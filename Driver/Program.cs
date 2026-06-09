using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


BenchmarkRunner.Run<MatcherBenchmark>();


 
// 2. МОДЕЛИ ДАННЫХ
 
public record Coordinate(int X, int Y);
public record Driver(int Id, Coordinate Location);
public record Order(Coordinate Location);

public interface IDriverMatcher
{
    List<Driver> FindTopDrivers(Order order, List<Driver> drivers, int count = 5);
}

public static class DistanceCalculator
{
    public static double GetDistance(Coordinate c1, Coordinate c2)
    {
        return Math.Sqrt(Math.Pow(c1.X - c2.X, 2) + Math.Pow(c1.Y - c2.Y, 2));
    }
}


 
// 3. ТРИ АЛГОРИТМА ПОИСКА
 

// Алгоритм 1: Линейный перебор (Brute Force)
public class BruteForceMatcher : IDriverMatcher
{
    public List<Driver> FindTopDrivers(Order order, List<Driver> drivers, int count = 5)
    {
        return drivers
            .OrderBy(d => DistanceCalculator.GetDistance(order.Location, d.Location))
            .Take(count)
            .ToList();
    }
}

// Алгоритм 2: Очередь с приоритетом (Priority Queue)
public class PriorityQueueMatcher : IDriverMatcher
{
    public List<Driver> FindTopDrivers(Order order, List<Driver> drivers, int count = 5)
    {
        var pq = new PriorityQueue<Driver, double>();

        foreach (var driver in drivers)
        {
            double dist = DistanceCalculator.GetDistance(order.Location, driver.Location);
            pq.Enqueue(driver, dist);
        }

        var result = new List<Driver>();
        while (pq.Count > 0 && result.Count < count)
        {
            result.Add(pq.Dequeue());
        }
        return result;
    }
}

// Алгоритм 3: Пространственная сетка (Grid Bucket)
public class GridBucketMatcher : IDriverMatcher
{
    private readonly int _cellSize;
    public GridBucketMatcher(int cellSize = 50) => _cellSize = cellSize;

    public List<Driver> FindTopDrivers(Order order, List<Driver> drivers, int count = 5)
    {
        var grid = new Dictionary<(int, int), List<Driver>>();
        foreach (var d in drivers)
        {
            var key = (d.Location.X / _cellSize, d.Location.Y / _cellSize);
            if (!grid.ContainsKey(key)) grid[key] = new List<Driver>();
            grid[key].Add(d);
        }

        int orderCellX = order.Location.X / _cellSize;
        int orderCellY = order.Location.Y / _cellSize;

        var candidates = new List<Driver>();
        int radius = 0;

        while (candidates.Count < count && radius < 10)
        {
            candidates.Clear();
            for (int x = orderCellX - radius; x <= orderCellX + radius; x++)
            {
                for (int y = orderCellY - radius; y <= orderCellY + radius; y++)
                {
                    // Исправлено: заменено ошибочное WhiteSpace на корректный out
                    if (grid.TryGetValue((x, y), out var cellDrivers))
                    {
                        candidates.AddRange(cellDrivers);
                    }
                }
            }
            radius++;
        }

        return candidates
            .OrderBy(d => DistanceCalculator.GetDistance(order.Location, d.Location))
            .Take(count)
            .ToList();
    }
}



// БЕНЧМАРК

[MemoryDiagnoser]
public class MatcherBenchmark
{
    private List<Driver> _drivers = new();
    private Order _order = null!;

    private readonly BruteForceMatcher _bruteForce = new();
    private readonly PriorityQueueMatcher _priorityQueue = new();
    private readonly GridBucketMatcher _gridBucket = new(cellSize: 50);

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _drivers = new List<Driver>();

        for (int i = 0; i < 10000; i++)
        {
            _drivers.Add(new Driver(i, new Coordinate(random.Next(0, 1000), random.Next(0, 1000))));
        }

        _order = new Order(new Coordinate(500, 500));
    }

    [Benchmark]
    public List<Driver> Тест_ЛинейныйПоиск() => _bruteForce.FindTopDrivers(_order, _drivers);

    [Benchmark]
    public List<Driver> Тест_ОчередьПриоритетов() => _priorityQueue.FindTopDrivers(_order, _drivers);

    [Benchmark]
    public List<Driver> Тест_ПространственнаяСетка() => _gridBucket.FindTopDrivers(_order, _drivers);
}