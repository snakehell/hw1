using System;
using System.Numerics;
using System.Collections.Generic;

namespace Project
{
    struct DataItem
    {
        public double x { get; set; }
        public double y { get; set; }
        public Complex Vector { get; set; }
        public DataItem(double x, double y, Complex Vector)
        {
            this.x = x;
            this.y = y;
            this.Vector = Vector;
        }
        public string ToLongString(string format)
        {
            return String.Format(format, this.x, this.y, this.Vector, this.Vector.Magnitude);
        }
        public override string ToString()
        {
            return String.Format("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}",
                this.x, this.y, this.Vector, this.Vector.Magnitude);
        }
    }

    delegate Complex FdblComplex(double x, double y);

    abstract class V1Data
    {
        public string obj { get; }
        public DateTime data { get; }

        public V1Data(string obj, DateTime data)
        {
            this.obj = obj;
            this.data = data;
        }
        public abstract int Count { get; }
        public abstract double AverageValue { get; }
        public abstract string ToLongString(string format);
        public abstract override string ToString();
    }

    class V1DataList : V1Data
    {
        public List<DataItem> DataList { get; }
        public V1DataList(string obj, DateTime data) : base(obj, data)
        {
            DataList = new List<DataItem>();
        }
        public bool Add(DataItem newItem)
        {
            foreach (DataItem Item in DataList)
            {
                if (Item.x == newItem.x && Item.y == newItem.y)
                {
                    return false;
                }
            }
            DataList.Add(newItem);
            return true;
        }

        public int AddDefaults(int nItems, FdblComplex F)
        {
            int count = 0;
            for (int i = 0; i < nItems; i++)
            {
                double x = i * 15;
                double y = i * 12;
                DataItem newItem = new DataItem(x, y, F(x, y));
                if (this.Add(newItem))
                {
                    count++;
                };
            }
            return count;
        }

        public override int Count
        {
            get { return DataList.Count; }
        }

        public override double AverageValue
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }
                double sum = 0.0;
                foreach (DataItem Item in DataList)
                {
                    sum += Item.Vector.Magnitude;
                }
                return sum / Count;
            }
        }

        public override string ToLongString(string format)
        {
            string str1 = String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3}\n", "V1DataList", obj, data, this.Count) + '\n';
            string str2 = "";
            foreach (DataItem Item in DataList)
            {
                str2 += String.Format(format, Item.x, Item.y, Item.Vector, Item.Vector.Magnitude);
            }
            return str1 + str2 + '\n';
        }
        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Count:{3}\n", "V1DataList", obj, data, this.Count) + '\n';
        }

    }

    class V1DataArray : V1Data
    {
        public int Knot_cnt_ox { get; }
        public int Knot_cnt_oy { get; }
        public double Step_ox { get; }
        public double Step_oy { get; }
        public Complex[,] Array { get; }

        public V1DataArray(string obj, DateTime data) : base(obj, data)
        {
            Array = new Complex[0, 0];
        }
        public V1DataArray(string obj, DateTime data, int Knot_cnt_ox, int Knot_cnt_oy, double Step_ox, double Step_oy, FdblComplex F) : base(obj, data)
        {
            this.Knot_cnt_ox = Knot_cnt_ox;
            this.Knot_cnt_oy = Knot_cnt_oy;
            this.Step_ox = Step_ox;
            this.Step_oy = Step_oy;
            Array = new Complex[Knot_cnt_ox, Knot_cnt_oy];
            for (int i = 0; i < Knot_cnt_ox; i++)
            {
                for (int j = 0; j < Knot_cnt_oy; j++)
                {
                    Array[i, j] = F(i * Step_ox, j * Step_oy);
                }
            }
        }
        public override int Count
        {
            get
            {
                return Knot_cnt_ox * Knot_cnt_oy;
            }
        }

        public override double AverageValue
        {
            get
            {
                if (Count == 0)
                {
                    return 0;
                }
                double sum = 0.0;
                for (int i = 0; i < Knot_cnt_ox; i++)
                {
                    for (int j = 0; j < Knot_cnt_oy; j++)
                    {
                         sum += Array[i, j].Magnitude;
                    }
                }
                return sum / Count;
            }
        }

        public override string ToString()
        {
            return String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Knot_cnt_ox:{3}\n Knot_cnt_oy:{4}\n Step_ox:{5}\n Step_oy:{6}\n", "V1DataArray", obj, data, Knot_cnt_ox, Knot_cnt_oy, Step_ox, Step_oy) + '\n';
        }

        public override string ToLongString(string format)
        {
            string str1 = String.Format("ClassName:{0}\n obj:{1}\n data:{2}\n Knot_cnt_ox:{3}\n Knot_cnt_oy:{4}\n Step_ox:{5}\n Step_oy:{6}\n", "V1DataArray", obj, data, Knot_cnt_ox, Knot_cnt_oy, Step_ox, Step_oy) + '\n';
            string str2 = "";
            for (int i = 0; i < Knot_cnt_ox; i++)
            {
                for (int j = 0; j < Knot_cnt_oy; j++)
                {
                    str2 += String.Format(format, i * Step_ox, j * Step_oy, Array[i, j], Array[i, j].Magnitude);
                }
            }
            return str1 + str2 + '\n';
        }
        public V1DataList ArrayToList()
        {
            V1DataList DataList = new V1DataList(this.obj, this.data);
            for (int i = 0; i < Knot_cnt_ox; i++)
            {
                for (int j = 0; j < Knot_cnt_oy; j++)
                {
                    double Ox = i * Step_ox;
                    double Oy = j * Step_oy;
                    Complex value = Array[i, j];
                    DataItem Item = new DataItem(Ox, Oy, value);
                    DataList.Add(Item);
                }
            }
            return DataList;
        }
    }

    class V1MainCollection
    {
        private List<V1Data> Collection = new List<V1Data>();
        public int Count()
        {
            return Collection.Count;
        }
        public V1Data this[int index]
        {
            get
            {
                return Collection[index];
            }
        }
        public bool Contains(string ID)
        {
            foreach (V1Data Data in Collection)
            {
                if (Data.obj == ID)
                {
                    return true;
                }
            }
            return false;
        }
        public bool Add(V1Data v1Data)
        {
            if (!this.Contains(v1Data.obj))
            {
                Collection.Add(v1Data);
                return true;
            }
            return false;
        }
        public string ToLongString(string format)
        {
            string str = "";
            foreach (V1Data Item in Collection)
            {
                str += Item.ToLongString(format);
            }
            return str;
        }
    }

    static class Method
    {
        static public Complex E(double X, double Y)
        {
            return new Complex(X + Y, Y - X);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            FdblComplex F = Method.E;
            V1DataArray arr1 = new V1DataArray("V1DataArray1", new DateTime(1, 1, 1), 1, 5, 1, 2, F);
            Console.WriteLine(arr1.ToLongString("{0:f2} {1:f2} {2} {3:f2}\n"));
            V1DataList list1 = arr1.ArrayToList();
            Console.WriteLine(list1.ToLongString("{0:f2} {1:f2} {2} {3:f2}\n"));
            Console.WriteLine(" ArrCount: {0}\n ArrAverageValue: {1:f2}\n ListCount: {2}\n ListAverageValue: {3:f2}\n", arr1.Count, arr1.AverageValue, list1.Count, list1.AverageValue);
            
            V1MainCollection collection = new V1MainCollection();
            collection.Add(list1);
            collection.Add(arr1);
            V1DataArray arr2 = new V1DataArray("V1DataArray2", new DateTime(1, 1, 1), 1, 7, 1, 3, F);
            collection.Add(arr2);
            V1DataList list2 = new V1DataList("List2", DateTime.Now);
            list2.AddDefaults(10, F);
            collection.Add(list2);
            Console.WriteLine(collection.ToLongString("{0:f2} {1:f2} {2} {3:f2}\n"));
            
            for (int i = 0; i < collection.Count(); i++)
            {
                Console.WriteLine("Count {0:f2} AverageValue {1:f2}", collection[i].Count, collection[i].AverageValue);
            }
        }
    }
}
