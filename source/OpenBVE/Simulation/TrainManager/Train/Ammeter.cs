using System;

namespace OpenBve
{
    internal class Ammeter
    {
        private TrainManager.Train Train;

        /// <summary>The reference ammeter table at zero loading</summary>
        internal AmmeterTable[] noLoadTable;

        /// <summary>The reference ammeter table at maximum loading</summary>
        internal AmmeterTable[] maxLoadTable;

        /// <summary>The ammeter table to be used with no current draw (e.g. Notch 0 or reverser in N) </summary>
        internal AmmeterTable[] noCurrentTable;

        internal Ammeter(TrainManager.Train train)
        {
            this.Train = train;
            this.noLoadTable = new AmmeterTable[0];
            this.maxLoadTable = new AmmeterTable[0];
            this.noCurrentTable = new AmmeterTable[0];
        }

        internal double CurrentValue()
        {
            int Power = this.Train.Specs.CurrentPowerNotch.Safety;
            int Reverser = this.Train.Specs.CurrentReverser.Actual;
            int Brake = this.Train.Specs.CurrentBrakeNotch.Actual;

            if (Power == 0 || Reverser == 0)
            {
                if (this.noCurrentTable.Length == 0)
                {
                    return 0;
                }
                for (int i = 0; i < this.noCurrentTable.Length; i++)
                {
                    if (Math.Round(this.Train.Cars[0].Specs.CurrentSpeed, 0) == this.noCurrentTable[i].setSpeed)
                    {
                        return this.noCurrentTable[i].tableValues[0];
                    }
                }
            }
            else
            {
                double unloadedValue = 0;
                double loadedValue = 0;

                for (int i = 0; i < this.noLoadTable.Length; i++)
                {
                    if (Math.Round(this.Train.Cars[0].Specs.CurrentSpeed, 0) == this.noLoadTable[i].setSpeed)
                    {
                        unloadedValue = this.noLoadTable[i].tableValues[Math.Min(Brake, this.noLoadTable[i].tableValues.Length)];
                    }
                }
                for (int i = 0; i < this.maxLoadTable.Length; i++)
                {
                    if (Math.Round(this.Train.Cars[0].Specs.CurrentSpeed, 0) == this.maxLoadTable[i].setSpeed)
                    {
                        //loadedValue = this.maxLoadTable[i].tableValues[Math.Min(Brake, this.maxLoadTable[i].tableValues.Length)];
                        return this.maxLoadTable[i].tableValues[Math.Min(Brake, this.maxLoadTable[i].tableValues.Length)];
                    }
                }
            }
            return 0;
        }
    }

    struct AmmeterTable
    {
        /// <summary>The set speed for this table entry</summary>
        internal double setSpeed;
        internal double[] tableValues;
        internal AmmeterTable(double speed, double[] values)
        {
            this.setSpeed = speed;
            this.tableValues = values;
        }
    }
}