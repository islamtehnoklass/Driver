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
        // Создаем тестовую карту: заказ в точке (0,0)
        _order = new Order(new Coordinate(0, 0));

        // Специально создаем водителей на разном расстоянии:
        // Водитель 1 — самый близкий (расстояние 1)
        // Водитель 2 — чуть дальше (расстояние 2)
        // Водитель 3 — самый дальний (расстояние 10)
        _drivers = new List<Driver>
        {
            new Driver(3, new Coordinate(0, 10)),
            new Driver(1, new Coordinate(0, 1)),
            new Driver(2, new Coordinate(0, 2))
        };
    }

    [Test]
    public void BruteForce_ShouldFindClosestDriverFirst()
    {
        var matcher = new BruteForceMatcher();
        var result = matcher.FindTopDrivers(_order, _drivers, count: 1);

        // Проверяем, что вернулся именно Водитель с Id = 1
        Assert.That(result[0].Id, Is.EqualTo(1));
    }

    [Test]
    public void PriorityQueue_ShouldSortDriversByDistance()
    {
        var matcher = new PriorityQueueMatcher();
        var result = matcher.FindTopDrivers(_order, _drivers, count: 2);

        // Проверяем, что вернулись самые близкие: сначала Id 1, потом Id 2
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
    }

    [Test]
    public void GridBucket_ShouldFindClosestDriversInRadius()
    {
        // Передаем размер сетки (например, 50)
        var matcher = new GridBucketMatcher(cellSize: 50);
        var result = matcher.FindTopDrivers(_order, _drivers, count: 2);

        // Проверяем, что пространственная сетка тоже отработала верно: сначала Id 1, потом Id 2
        Assert.That(result[0].Id, Is.EqualTo(1));
        Assert.That(result[1].Id, Is.EqualTo(2));
    }
}