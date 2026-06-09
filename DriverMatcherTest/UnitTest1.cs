using NUnit.Framework;
using System.Collections.Generic;

namespace DriverMatcher.Tests;

[TestFixture]
public class MatcherTests
{
    private List<Driver> _drivers = null!;
    private Order _order = null!;

    [SetUp]
    public void Setup()
    {
        // Заказ в точке (0,0) -> ячейка (0,0)
        _order = new Order(new Coordinate(0, 0));

        // Размер ячейки у нас 50.
        // Сделаем так, чтобы близкие были в ячейке (0,0), а дальний - вообще в другой ячейке!
        _drivers = new List<Driver>
        {
            new Driver(2, new Coordinate(0, 20)),  // Близкий 2 (ячейка 0,0)
            new Driver(1, new Coordinate(0, 5)),   // Самый близкий 1 (ячейка 0,0)
            new Driver(3, new Coordinate(0, 200))  // Очень дальний 3 (ячейка 0,4) - сетка его сразу отсечет
        };
    }

    [Test]
    public void BruteForce_ShouldFindClosestDriverFirst()
    {
        var matcher = new BruteForceMatcher();
        var result = matcher.FindTopDrivers(_order, _drivers, count: 1);
        Assert.That(result[0].Id, Is.EqualTo(1));
    }

    [Test]
    public void PriorityQueue_ShouldSortDriversByDistance()
    {
        var matcher = new PriorityQueueMatcher();
        var result = matcher.FindTopDrivers(_order, _drivers, count: 2);
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
    }

    [Test]
    public void GridBucket_ShouldFindClosestDriversInRadius()
    {
        var matcher = new GridBucketMatcher(cellSize: 50);
        var result = matcher.FindTopDrivers(_order, _drivers, count: 2);

        // Теперь в радиус первой ячейки гарантированно попадут только Id 1 и Id 2
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
    }
}