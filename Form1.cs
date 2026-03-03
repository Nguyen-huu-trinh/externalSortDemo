using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Sắp_xếp_ngoại
{
    public partial class Form1 : Form
    {
        private List<double> data = new List<double>();

        public Form1()
        {
            InitializeComponent();
        }


        // Đọc dữ liệu và hiển thị lên RichTextBox + Chart
        private void ShowFileData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File không tồn tại: " + filePath);
                return;
            }

            richTextBox1.Clear();
            data.Clear();
            using (BinaryReader br = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                List<string> values = new List<string>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    double value = br.ReadDouble();
                    values.Add(value.ToString("F2")); // định dạng 2 chữ số thập phân
                    data.Add(value);// Thêm vào danh sách
                }

                // ghép thành một chuỗi ngang
                string line = string.Join("  ", values);
                richTextBox1.AppendText(line);

            }

            ShowChart(); // hiển thị dữ liệu bằng biểu đồ
        }

        private string selectedFile = "";
        //nút chọn file
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedFile = ofd.FileName;
                MessageBox.Show("Đã chọn file: " + selectedFile);

                // Hiển thị dữ liệu gốc trong file
                ShowFileData(selectedFile);
            }


        }

        // Hiển thị dữ liệu lên Chart1 (minh họa quá trình sắp xếp)
        private void ShowData(List<double> arr)
        {
            // Xóa series cũ
            chart1.Series.Clear();

            // Tạo series mới
            var series = chart1.Series.Add("Data");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            // Thêm dữ liệu vào chart: trục X là vị trí, trục Y là giá trị
            for (int i = 0; i < arr.Count; i++)
            {
                series.Points.AddXY(i, arr[i]);
            }
            //hiệu ứng minh họa: cập nhật ngay và dừng ngắn để thấy quá trình
            Application.DoEvents();
            System.Threading.Thread.Sleep(300);



        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        struct MinHeapNode
        {
            public double element; // phần tử lưu trữ
            public int i;          // chỉ số file run mà phần tử này thuộc về
        }

        class MinHeap
        {
            private MinHeapNode[] harr; // mảng heap
            private int heap_size;      // kích thước heap

            public MinHeap(MinHeapNode[] a, int size)
            {
                heap_size = size;
                harr = a;
                int i = (heap_size - 1) / 2;
                while (i >= 0)
                {
                    MinHeapify(i);
                    i--;
                }
            }

            private void MinHeapify(int i)
            {
                int l = left(i);
                int r = right(i);
                int smallest = i;

                if (l < heap_size && harr[l].element < harr[i].element)
                    smallest = l;

                if (r < heap_size && harr[r].element < harr[smallest].element)
                    smallest = r;

                if (smallest != i)
                {
                    Swap(ref harr[i], ref harr[smallest]);
                    MinHeapify(smallest);
                }
            }

            private int left(int i) { return (2 * i + 1); }
            private int right(int i) { return (2 * i + 2); }

            public MinHeapNode GetMin() { return harr[0]; }

            public void ReplaceMin(MinHeapNode x)
            {
                harr[0] = x;
                MinHeapify(0);
            }

            private void Swap(ref MinHeapNode x, ref MinHeapNode y)
            {
                MinHeapNode temp = x;
                x = y;
                y = temp;
            }
        }

        // MergeSort cho mảng nhỏ
        static void MergeSort(double[] arr, int l, int r)
        {
            if (l < r)
            {
                int m = l + (r - l) / 2;
                MergeSort(arr, l, m);
                MergeSort(arr, m + 1, r);
                Merge(arr, l, m, r);
            }
        }
        // Hàm Merge trong MergeSort
        static void Merge(double[] arr, int l, int m, int r)
        {
            int n1 = m - l + 1;
            int n2 = r - m;
            double[] L = new double[n1];
            double[] R = new double[n2];

            for (int i = 0; i < n1; i++) L[i] = arr[l + i];
            for (int j = 0; j < n2; j++) R[j] = arr[m + 1 + j];

            int ii = 0, jj = 0, k = l;
            while (ii < n1 && jj < n2)
            {
                if (L[ii] <= R[jj]) arr[k++] = L[ii++];
                else arr[k++] = R[jj++];
            }
            while (ii < n1) arr[k++] = L[ii++];
            while (jj < n2) arr[k++] = R[jj++];
        }

        // Tạo các runs ban đầu
        private int CreateInitialRuns(string inputFile, int run_size)
        {
            int runIndex = 0;
            using (BinaryReader br = new BinaryReader(File.Open(inputFile, FileMode.Open)))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    List<double> buffer = new List<double>();
                    for (int i = 0; i < run_size && br.BaseStream.Position < br.BaseStream.Length; i++)
                    {
                        buffer.Add(br.ReadDouble());
                    }

                    double[] arr = buffer.ToArray();
                    MergeSort(arr, 0, arr.Length - 1);//sắp xếp run

                    string runFile = $"run{runIndex}.bin";
                    using (BinaryWriter bw = new BinaryWriter(File.Open(runFile, FileMode.Create)))
                    {
                        foreach (double x in arr) bw.Write(x);
                    }

                    ShowData(arr.ToList());//Hiển thị run đã sắp xếp

                    runIndex++;
                }
            }
            return runIndex;
        }


        // Trộn các runs bằng MinHeap
        private void MergeFiles(string outputFile, int runCount)
        {
            BinaryReader[] readers = new BinaryReader[runCount];
            for (int i = 0; i < runCount; i++)
                readers[i] = new BinaryReader(File.Open($"run{i}.bin", FileMode.Open));

            using (BinaryWriter bw = new BinaryWriter(File.Open(outputFile, FileMode.Create)))
            {
                MinHeapNode[] harr = new MinHeapNode[runCount];
                int i;
                for (i = 0; i < runCount; i++)
                {
                    if (readers[i].BaseStream.Position < readers[i].BaseStream.Length)
                    {
                        harr[i].element = readers[i].ReadDouble();
                        harr[i].i = i;
                    }
                    else break;
                }

                MinHeap hp = new MinHeap(harr, i);
                int count = 0;
                List<double> mergedSoFar = new List<double>();

                while (count != i)
                {
                    MinHeapNode root = hp.GetMin();
                    bw.Write(root.element);

                    mergedSoFar.Add(root.element);

                    // Vẽ dãy kết quả đang hình thành trên Chart1
                    ShowData(mergedSoFar);

                    if (readers[root.i].BaseStream.Position < readers[root.i].BaseStream.Length)
                    {
                        root.element = readers[root.i].ReadDouble();
                    }
                    else
                    {
                        root.element = double.MaxValue;
                        count++;
                    }
                    hp.ReplaceMin(root);
                }
            }

            foreach (var r in readers) r.Close();
        }

        // ExternalSort tạo run và trộn lại
        private void ExternalSort(string inputFile, string outputFile, int run_size)
        {
            int runCount = CreateInitialRuns(inputFile, run_size);
            MergeFiles(outputFile, runCount);
        }


        //nút External Sort
        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFile))
            {
                MessageBox.Show("Bạn chưa chọn file! Hãy nhấn Open File trước.");
                return;
            }

            string outputFile = "sorted.bin";
            int run_size = 5; // số phần tử mỗi run

            ExternalSort(selectedFile, outputFile, run_size);
            MessageBox.Show("Đã sắp xếp xong! File kết quả: " + outputFile);

            // Hiển thị dữ liệu đã sắp xếp
            ShowFileData(outputFile);

        }






        private void chart1_Click(object sender, EventArgs e)
        {

        }
        private void ShowChart()
        {
            chart1.Series.Clear();
            var series = chart1.Series.Add("Data");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            foreach (double x in data)
            {
                series.Points.Add(x);
            }
        }


    }
}

