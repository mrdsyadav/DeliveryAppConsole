using System;
using System.Collections.Generic;
using System.Linq;

namespace DeliveryApp
{
    public class Program
    {
        public static Dictionary<string, Offer> offers = new Dictionary<string, Offer>()
        {
            {"OFR001", new Offer{Code="OFR001",DiscountPercent=0.10,MinWeight=70,MaxWeight=200,MinDistance=0,MaxDistance=200}},
            {"OFR002", new Offer{Code="OFR002",DiscountPercent=0.07,MinWeight=100,MaxWeight=250,MinDistance=50,MaxDistance=150}},
            {"OFR003", new Offer{Code="OFR003",DiscountPercent=0.05,MinWeight=10,MaxWeight=150,MinDistance=50,MaxDistance=250}}
        };

        public static void Main(string[] args)
        {
            try
            {
                var firstLine = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(firstLine))
                {
                    Console.WriteLine("Invalid input. Please enter base cost and package count.");
                    return;
                }

                var first = firstLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (first.Length != 2 ||
                    !int.TryParse(first[0], out int baseCost) ||
                    !int.TryParse(first[1], out int n))
                {
                    Console.WriteLine("Invalid base cost or package count.");
                    return;
                }

                if (baseCost < 0 || n <= 0)
                {
                    Console.WriteLine("Base cost must be >=0 and package count >0.");
                    return;
                }

                List<Package> packages = new List<Package>();

                for (int i = 0; i < n; i++)
                {
                    var line = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Console.WriteLine("Invalid package input.");
                        return;
                    }

                    var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (p.Length != 4)
                    {
                        Console.WriteLine($"Invalid package format at line {i + 1}");
                        return;
                    }

                    if (!int.TryParse(p[1], out int weight) ||
                        !int.TryParse(p[2], out int distance))
                    {
                        Console.WriteLine($"Invalid weight or distance in package {p[0]}");
                        return;
                    }

                    if (weight <= 0 || distance <= 0)
                    {
                        Console.WriteLine($"Weight and distance must be positive for package {p[0]}");
                        return;
                    }

                    packages.Add(new Package
                    {
                        Id = p[0],
                        Weight = weight,
                        Distance = distance,
                        OfferCode = p[3]
                    });
                }

                CalculateCost(baseCost, packages);

                var vehicleLine = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(vehicleLine))
                {
                    Console.WriteLine("Vehicle input missing.");
                    return;
                }

                var vehicleInput = vehicleLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (vehicleInput.Length != 3 ||
                    !int.TryParse(vehicleInput[0], out int vehicleCount) ||
                    !int.TryParse(vehicleInput[1], out int speed) ||
                    !int.TryParse(vehicleInput[2], out int maxWeight))
                {
                    Console.WriteLine("Invalid vehicle input.");
                    return;
                }

                if (vehicleCount <= 0 || speed <= 0 || maxWeight <= 0)
                {
                    Console.WriteLine("Vehicle count, speed, and max weight must be positive.");
                    return;
                }

                CalculateDeliveryTime(packages, vehicleCount, speed, maxWeight);

                foreach (var p in packages)
                {
                    Console.WriteLine($"{p.Id} {p.Discount} {p.TotalCost} {p.DeliveryTime:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.Message);
            }

            Console.ReadLine();
        }

        public static void CalculateCost(int baseCost, List<Package> packages)
        {
            foreach (var p in packages)
            {
                double cost = baseCost + (p.Weight * 10) + (p.Distance * 5);
                double discount = 0;

                if (offers.ContainsKey(p.OfferCode))
                {
                    var offer = offers[p.OfferCode];

                    if (p.Weight >= offer.MinWeight &&
                        p.Weight <= offer.MaxWeight &&
                        p.Distance >= offer.MinDistance &&
                        p.Distance <= offer.MaxDistance)
                    {
                        discount = cost * offer.DiscountPercent;
                    }
                }

                p.Discount = Math.Round(discount, 2);
                p.TotalCost = Math.Round(cost - discount, 2);
            }
        }

        public static void CalculateDeliveryTime(List<Package> packages, int vehicleCount, int speed, int maxWeight)
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            for (int i = 0; i < vehicleCount; i++)
                vehicles.Add(new Vehicle { Id = i });

            while (packages.Any(x => !x.Delivered))
            {
                var vehicle = vehicles.OrderBy(v => v.AvailableAt).First();

                var remaining = packages
                    .Where(x => !x.Delivered)
                    .OrderByDescending(x => x.Weight)
                    .ThenBy(x => x.Distance)
                    .ToList();

                List<Package> shipment = new List<Package>();
                int weight = 0;

                foreach (var p in remaining)
                {
                    if (weight + p.Weight <= maxWeight)
                    {
                        shipment.Add(p);
                        weight += p.Weight;
                    }
                }

                if (shipment.Count == 0)
                    shipment.Add(remaining.First());

                double maxTripTime = 0;

                foreach (var p in shipment)
                {
                    double time = (double)p.Distance / speed;

                    p.DeliveryTime = Math.Round(vehicle.AvailableAt + time, 2);

                    maxTripTime = Math.Max(maxTripTime, time);

                    p.Delivered = true;
                }

                vehicle.AvailableAt += maxTripTime * 2;
            }
        }
    }
}