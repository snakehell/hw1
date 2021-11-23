using System;
using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    abstract class V1Data : IEnumerable<DataItem>
    {
        public string obj { get; protected set; }
        public DateTime data { get; protected set; }

        public V1Data(string obj, DateTime data)
        {
            this.obj = obj;
            this.data = data;
        }
        public abstract int Count { get; }
        public abstract double AverageValue { get; }
        public abstract string ToLongString(string format);
        public abstract override string ToString();
        protected abstract IEnumerator GetEnumerator();
        protected abstract IEnumerator<DataItem> GetEnumerator_DataItem();
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return (IEnumerator<DataItem>) GetEnumerator_DataItem();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) GetEnumerator();
        }
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
        protected override IEnumerator GetEnumerator()
        {
            return (IEnumerator) DataList.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return (IEnumerator<DataItem>) DataList.GetEnumerator();
        }
        public bool SaveAsText(string filename)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine(obj);
                    writer.WriteLine(data);
                    writer.WriteLine(Count);
                    foreach (DataItem item in DataList)
                    {
                        writer.WriteLine(item.x);
                        writer.WriteLine(item.y);
                        writer.WriteLine(item.Vector.Real);
                        writer.WriteLine(item.Vector.Imaginary);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadAsText(string filename)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    obj = reader.ReadLine();
                    DateTime data = DateTime.Parse(reader.ReadLine());
                    int count = int.Parse(reader.ReadLine());
                    for (int i = 0; i < count; i++)
                    {
                        DataItem newitem = new DataItem(double.Parse(reader.ReadLine()), double.Parse(reader.ReadLine()), new Complex(double.Parse(reader.ReadLine()), double.Parse(reader.ReadLine())));
                        this.Add(newitem);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }

    class V1DataArray : V1Data
    {
        public int Knot_cnt_ox { get; private set; }
        public int Knot_cnt_oy { get; private set; }
        public double Step_ox { get; private set; }
        public double Step_oy { get; private set; }
        public Complex[,] Array { get; private set; }

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
        protected override IEnumerator GetEnumerator()
        {
            return (IEnumerator) this.ArrayToList().DataList.GetEnumerator();
        }
        protected override IEnumerator<DataItem> GetEnumerator_DataItem()
        {
            return (IEnumerator<DataItem>) this.ArrayToList().DataList.GetEnumerator();
        }
        public bool SaveBinary(string filename)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    writer.Write(obj);
                    writer.Write(data.ToBinary());
                    writer.Write(Knot_cnt_ox);
                    writer.Write(Knot_cnt_oy);
                    writer.Write(Step_ox);
                    writer.Write(Step_oy);
                    foreach (Complex vector in Array)
                    {
                        writer.Write(vector.Real);
                        writer.Write(vector.Imaginary);
                    };
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        public bool LoadBinary(string filename)
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    obj = reader.ReadString();
                    data = DateTime.FromBinary(reader.ReadInt64());
                    Knot_cnt_ox = reader.ReadInt32();
                    Knot_cnt_oy = reader.ReadInt32();
                    Step_ox = reader.ReadDouble();
                    Step_oy = reader.ReadDouble();
                    Array = new Complex[Knot_cnt_ox, Knot_cnt_oy];
                    for (int i = 0; i < Knot_cnt_ox; i++)
                    {
                        for (int j = 0; j < Knot_cnt_oy; j++)
                        {
                            Array[i, j] = new Complex(reader.ReadDouble(), reader.ReadDouble());
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
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
        public double AverageValueForArrays
        {
            get
            {
                if (Collection.Count == 0)
                    return double.NaN;
                IEnumerable<DataItem> items = from elem in Collection 
                                              from item in elem 
                                              select item;
                IEnumerable<double> magnitude = from i in items 
                                                select i.Vector.Magnitude;
                if (magnitude != null && magnitude.Any())
                    return magnitude.Average();
                else
                    return double.NaN;
            }
        }
        public DataItem? MaxDev
        {
            get
            {
                if (Collection.Count == 0)
                    return null;
                IEnumerable<DataItem> items = from elem in Collection
                                              from item in elem
                                              select item;
                if (items != null && items.Any())
                    return items.OrderByDescending(x => Math.Abs(x.Vector.Magnitude - AverageValueForArrays)).First();
                else
                    return null;
            }
        }
        public IEnumerable<float> ManyX
        {
            get
            {
                if (Collection.Count == 0)
                    return null;

                IEnumerable<DataItem> items = from elem in Collection
                                              from item in elem
                                              select item;
                IEnumerable<double> x = from i in items
                                             select i.x;
                IEnumerable<IGrouping<double, double>> group = x.GroupBy(x => x);
                IEnumerable<float> mx = from i in @group
                                            where i.Count() >= 2
                                            select (float)i.Key;
                if (mx != null && mx.Any())
                    return mx;
                else
                    return null;
            }
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
        static void Test1(string filename)
        {
            Console.WriteLine("Проверка записи и чтения\n");
            V1DataArray arr = new V1DataArray("test1", DateTime.Today, 2, 2, 1, 0.11, Method.E);
            arr.SaveBinary(filename);
            Console.WriteLine(arr.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));
            V1DataArray copy_arr = new V1DataArray("test1", DateTime.Today);
            copy_arr.LoadBinary(filename);
            Console.WriteLine(copy_arr.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));

            V1DataList list = new V1DataList("test2", DateTime.Today);
            list.AddDefaults(3, Method.E);
            list.SaveAsText(filename);
            Console.WriteLine(list.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));
            V1DataList copy_list = new V1DataList("test2", DateTime.Today);
            copy_list.LoadAsText(filename);
            Console.WriteLine(copy_list.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));

            Console.WriteLine("Сохраняем и востанавливаем пустой массив\n");
            V1DataArray arr1 = new V1DataArray("test0", DateTime.Today, 0, 2, 1, 0.11, Method.E);
            arr1.SaveBinary(filename);
            Console.WriteLine(arr1.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));
            V1DataArray copy_arr1 = new V1DataArray("test0", DateTime.Today);
            copy_arr1.LoadBinary(filename);
            Console.WriteLine(copy_arr1.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));

        }
        static void Test2()
        {
            try
            {
                Console.WriteLine("Проверка запросов\n");
                V1MainCollection collection = new V1MainCollection();
                collection.Add(new V1DataArray("test1", DateTime.Today, 2, 2, 100, 100, Method.E));
                collection.Add(new V1DataArray("test2", DateTime.Today, 0, 2, 0.2f, 0.5f, Method.E));
                V1DataList testList = new V1DataList("test3", DateTime.Today);
                testList.AddDefaults(0, Method.E);
                collection.Add(testList);
                V1DataList testList2 = new V1DataList("test4", DateTime.Today);
                testList2.AddDefaults(3, Method.E);
                collection.Add(testList2);
                Console.WriteLine(collection.ToLongString("X {0:f2} Y {1:f2} E_C {2} |E| {3:f2}\n"));
                Console.WriteLine("Среднее значение модуля поля для всех результатов измерений: ");
                Console.WriteLine(collection.AverageValueForArrays);
                Console.WriteLine("Объект с максимальным отклонением модуля поля от среднего значения: ");
                Console.WriteLine(collection.MaxDev);
                Console.WriteLine("Х координаты, встречающтеся более 1 раза в списке: ");
                foreach (float i in collection.ManyX)
                {
                    Console.WriteLine(i);
                }
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("Попытка вызвать методы пустой коллекции");
            }

        }
        static void Main(string[] args)
        {
            Test1("file.txt");
            Test2();
        }
    }
}
