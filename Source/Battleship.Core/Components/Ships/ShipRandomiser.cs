﻿namespace Battleship.Core.Components.Ships
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Enums;

    using Models;

    using Utilities;

    public class ShipRandomiser : ComponentBase,  IShipRandomiser
    {
        private static readonly Random Randomise = new Random();

        private static volatile ShipRandomiser instance;

        private readonly List<Segment> segments;

        private readonly int xMidPoint;

        private readonly int yMidPoint;

        private Coordinate coordinate;

        protected ShipRandomiser()
        {
            segments = new List<Segment>();

            this.Dimension = GridDimension;

            yMidPoint = GridDimension / 2;
            xMidPoint = (XInitialPoint + GridDimension / 2) - Index;
        }

        #region Properties

        public int Dimension { get; }

        #endregion

        public static ShipRandomiser Instance()
        {
            if (instance == null)
            {
                lock (SyncObject)
                {
                    if (instance == null)
                    {
                        instance = new ShipRandomiser();
                    }
                }
            }

            return instance;
        }

        #region IShipRandomiser Members

        public List<Segment> GetRandomisedShipCoordinates(IList<IShip> ships)
        {
            // Create a temporary segment list and pass it along by reference
            // Once done, we can clear it out and make sure the GC does its job
            List<Segment> temporarySegments = new List<Segment>();

            if (ships != null)
            {
                int totalShipLength = ships.Sum(q => q.ShipLength);

                if (totalShipLength != segments.Count)
                {
                    foreach (IShip ship in ships)
                    {
                        ShipDirection direction = Randomise.Next(this.Dimension) % 2 == 0
                                                      ? ShipDirection.Horizontal
                                                      : ShipDirection.Vertical;

                        while (temporarySegments != null && temporarySegments.Count != ship.ShipLength)
                        {
                            if (direction == ShipDirection.Horizontal)
                            {
                                this.MapXAxis(ship, ref temporarySegments);
                                if (temporarySegments.Count == ship.ShipLength)
                                {
                                    segments.AddRange(temporarySegments);
                                    temporarySegments.Clear();
                                    break;
                                }
                            }

                            if (direction == ShipDirection.Vertical)
                            { 
                                this.MapYAxis(ship, ref temporarySegments);

                                if (temporarySegments != null && temporarySegments.Count == ship.ShipLength)
                                {
                                    segments.AddRange(temporarySegments);
                                    temporarySegments.Clear();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return segments;
        }

        #endregion

        private void MapYAxis(IShip ship, ref List<Segment> temporarySegments)
        {
            coordinate = this.GenerateCoordinate();

            // Top is 1 to 5 and Bottom is 5 to 10 
            Boundaries orientation = coordinate.Y > yMidPoint ? Boundaries.Top : Boundaries.Bottom;

            if (orientation == Boundaries.Top)
            {
                for (int y = coordinate.Y; y > coordinate.Y - ship.ShipLength; y--)
                {
                    if (!this.AddToYAxis(coordinate.X, y, ship, ref temporarySegments))
                    {
                        break;
                    }
                }
            }

            if (orientation == Boundaries.Bottom)
            {
                for (int y = coordinate.Y; y < coordinate.Y + ship.ShipLength; y++)
                {
                    if (!this.AddToYAxis(coordinate.X, y, ship, ref temporarySegments))
                    {
                        break;
                    }
                }
            }
        }

        private void MapXAxis(IShip ship, ref List<Segment> temporarySegments)
        {
            coordinate = this.GenerateCoordinate();

            // Left to Right 65 to 69 Right to Left 74 to 70
            Boundaries orientation = coordinate.X >= xMidPoint ? Boundaries.RightToLeft : Boundaries.LeftToRight;

            if (orientation == Boundaries.LeftToRight)
            {
                for (int x = coordinate.X; x < coordinate.X + ship.ShipLength; x++)
                {
                    if (!this.AddToXAxis(x, coordinate.Y, ship, ref temporarySegments))
                    {
                        break;
                    }
                }
            }

            if (orientation == Boundaries.RightToLeft)
            {
                for (int x = coordinate.X; x > coordinate.X - ship.ShipLength; x--)
                {
                    if (!this.AddToXAxis(x, coordinate.Y, ship, ref temporarySegments))
                    {
                        break;
                    }
                }
            }
        }

        private bool AddToYAxis(int currentXPosition, int currentYPosition, IShip ship, ref List<Segment> temporarySegments)
        {
            bool result = false;

            // If the current segment position is valid add to the temporary list, otherwise clear the list and start again
            if (segments.IsSegmentAvailable(currentXPosition, currentYPosition)
                && BattleshipExtensions.IsSegmentWithInGridRange(currentXPosition, currentYPosition))
            {
                temporarySegments.Add(new Segment(currentXPosition, currentYPosition, ShipDirection.Vertical, ship));
                result = true;
            }
            else
            {
                this.Clear(temporarySegments);
            }

            return result;
        }

        private bool AddToXAxis(int currentXPosition, int currentYPosition, IShip ship, ref List<Segment> temporarySegments)
        {
            bool result = false;

            // If the current segment position is valid add to the temporary list, otherwise clear the list and start again
            if (segments.IsSegmentAvailable(currentXPosition, currentYPosition)
                && BattleshipExtensions.IsSegmentWithInGridRange(currentXPosition, currentYPosition))
            {
                temporarySegments.Add(new Segment(currentXPosition, currentYPosition, ShipDirection.Horizontal, ship));
                result = true;
            }
            else
            {
                this.Clear(temporarySegments);
            }

            return result;
        }

        private void Clear(List<Segment> shipCoordinates)
        {
            shipCoordinates.Clear();
        }

        private Coordinate GenerateCoordinate()
        {
            int positionX = Randomise.Next(XInitialPoint, XInitialPoint + GridDimension);
            int positionY = Randomise.Next(Index, this.Dimension);

             // if we hit the xMidPoint seed and add/subtract to positionX
            if (positionX == xMidPoint)
            {
                int seed = Randomise.Next(Index, XInitialPoint);
                positionX = seed % 2 == 0 ? positionX + seed : positionX - seed;
            }

            // if we hit the yMidPoint seed and add/subtract to positionY
            if (positionY == yMidPoint)
            {
                int seed = Randomise.Next(Index, yMidPoint);
                positionY = seed % 2 == 0 ? positionY + seed : positionY - seed;
            }

            coordinate = new Coordinate(positionX, positionY);
            return coordinate;
        }
    }
}