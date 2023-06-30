using System.Drawing.Imaging;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace Lab2
{
    public partial class Form1 : Form
    {
        private static int maxlabel;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image = new Bitmap(ofd.FileName);
                    pictureBox2.Image = null;
                }
                catch
                {
                    MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public static Bitmap BradleyThreshold(Bitmap image, int windowSize, double threshold,ref int[,] bytes)
        {
            Bitmap grayImage = Grayscale(image);
            Bitmap thresholdedImage = new Bitmap(grayImage.Width, grayImage.Height);
            int blockSize = (int)Math.Round(grayImage.Width / (double)windowSize);
            int[,] integralImage = new int[grayImage.Width, grayImage.Height];
            for (int y = 0; y < grayImage.Height; y++)
            {
                int rowSum = 0;
                for (int x = 0; x < grayImage.Width; x++)
                {
                    rowSum += grayImage.GetPixel(x, y).R;
                    if (y == 0)
                    {
                        integralImage[x, y] = rowSum;
                    }
                    else
                    {
                        integralImage[x, y] = integralImage[x, y - 1] + rowSum;
                    }
                }
            }
            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    int x1 = Math.Max(0, x - blockSize);
                    int y1 = Math.Max(0, y - blockSize);
                    int x2 = Math.Min(grayImage.Width - 1, x + blockSize);
                    int y2 = Math.Min(grayImage.Height - 1, y + blockSize);
                    int blockSum = integralImage[x2, y2] - integralImage[x1, y2] - integralImage[x2, y1] + integralImage[x1, y1];
                    double blockThreshold = blockSum / (blockSize * blockSize) * threshold;
                    if (grayImage.GetPixel(x, y).R > blockThreshold)
                    {
                        thresholdedImage.SetPixel(x, y, Color.White);
                        bytes[x, y] = 1;
                    }
                    else
                    {
                        thresholdedImage.SetPixel(x, y, Color.Black);
                        bytes[x, y] = 0;
                    }
                }
            }
            return thresholdedImage;
        }
        private static Bitmap Grayscale(Bitmap image)
        {
            Bitmap grayscaleImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color color = image.GetPixel(x, y);
                    int grayValue = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                    grayscaleImage.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }
            return grayscaleImage;
        }
        public static int[,] Labeling(int[,] bytes ,Bitmap img)
        {
            int[,] labels = new int[img.Width, img.Height];
            Array.Clear(labels, 0, labels.Length);
            int L = 1;
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    if (bytes[x,y] == 1 && labels[x,y] == 0)
                    Fill(bytes,img, ref labels, x, y, L++);
                }
            }
            maxlabel = L;
            return labels;
        }
        private static void Fill(int[,] bytes,Bitmap img, ref int[,] labels, int x, int y, int L)
        {
            int H = img.Height;
            int W = img.Width;
            if (labels[x, y] == 0 && bytes[x,y] == 1)
            {
                if (L == 1) Console.WriteLine();
                labels[x, y] = L;
                if (x > 0)
                    Fill(bytes,img, ref labels, x - 1, y, L);
                if (x < W - 1)
                    Fill(bytes,img, ref labels, x + 1, y, L);
                if (y > 0)
                    Fill(bytes,img, ref labels, x, y - 1, L);
                if (y < H - 1)
                    Fill(bytes,img, ref labels, x, y + 1, L);
            }
        }
        public static Bitmap DrawLabeledImage(Bitmap image, int[,] labels)
        {
            Bitmap labeledImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int label = labels[x, y];
                    Color color = label == 0 ? Color.Black : Color.FromArgb(label * 35 % 256, label * 5 % 256, label*20 % 256);
                    labeledImage.SetPixel(x, y, color);
                }
            }
            return labeledImage;
        }
        public Bitmap Apply(Bitmap image)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            for (int x = 0; x < image.Width - 1; x++)
            {
                for (int y = 0; y < image.Height - 1; y++)
                {
                    int gx = Math.Abs(image.GetPixel(x, y).R - image.GetPixel(x + 1, y + 1).R);
                    int gy = Math.Abs(image.GetPixel(x + 1, y).R - image.GetPixel(x, y + 1).R);
                    int g = gx + gy;
                    g = Math.Max(0, Math.Min(255, g));
                    result.SetPixel(x, y, Color.FromArgb(g, g, g));
                }
            }
            return result;
        }

        public  (double newX, double newY) GetNextPoint(double x, double y, double direction, double distance, string rightleft)
        {
            double newX, newY;
            if (rightleft == "Left")
            {
                newX = x - distance * Math.Cos(direction);
                newY = y - distance * Math.Sin(direction);
            }
            else
            {
                newX = x + distance * Math.Cos(direction);
                newY = y + distance * Math.Sin(direction);
            }
            return (newX, newY);
        }
        public string GetPrevandNextPoint(int[,] labels,double x, double y, double direction, double distance)
        {
            double newX = x, newY = y;
            double prevX = x , PrevY = y;
            string RightLeft;
            while (true)
            {
                if (labels[(int)Math.Round(prevX), (int)Math.Round(PrevY)] == 0)
                {
                    RightLeft = "Right";
                    break;
                }
                if (labels[(int)Math.Round(newX), (int)Math.Round(newY)] == 0)
                {
                    RightLeft = "Left";
                    break;
                }
                newX = newX + distance * Math.Cos(direction);
                newY = newY + distance * Math.Sin(direction);
                prevX = prevX - distance * Math.Cos(direction);
                PrevY = PrevY - distance * Math.Sin(direction);
            }
            return RightLeft;
        }
        public void FindStart(int[,] labels,Bitmap image,ref int label)
        {
            foreach (var i in Enumerable.Range(1, maxlabel))
            {
                for (int y = 0; y < labels.GetLength(1); y++)
                {
                    for (int x = 0; x < labels.GetLength(0); x++)
                    {
                        if (labels[x, y] == i && image.GetPixel(x,y).R > 200 && image.GetPixel(x,y).B < 5 && image.GetPixel(x,y).G < 5)
                        {
                            label = i;
                            return;
                        }
                    }
                }
            }
        }
        public bool IsArrow(int[,] labels,int label)
        {
            var comp = Geometriccharacteristics.GetCompactness(labels, label);
            var eccent = Geometriccharacteristics.GetEccentricity(labels, label);
            if(comp > 0.5 && comp < 0.8 && eccent > 0.8 && eccent < 0.9) return true;
            return false;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int[,] bytes = new int[new Bitmap(pictureBox1.Image).Width, new Bitmap(pictureBox1.Image).Height];
                pictureBox2.Image = BradleyThreshold(new Bitmap(pictureBox1.Image), 21, 0.15, ref bytes);
                var labels = Labeling(bytes, new Bitmap(pictureBox2.Image));
                pictureBox2.Image = pictureBox1.Image;
                Graphics g = Graphics.FromImage(pictureBox2.Image);
                int startlabel = 0;
                FindStart(labels, new Bitmap(pictureBox1.Image), ref startlabel);
                var tuple = Geometriccharacteristics.GetCentroid(labels, startlabel);
                var tuplenew = tuple;
                int x = tuple.Item1, y = tuple.Item2;
                for (int i = startlabel; ;)
                {
                    if (!IsArrow(labels, i))
                    {
                        g.DrawLine(new Pen(Color.Blue, 3), tuple.Item1, tuple.Item2, x, y);
                        Rectangle rectangle = new Rectangle(tuplenew.Item1-45, tuplenew.Item2-50, 100, 100);
                        g.DrawRectangle(new Pen(Color.White, 3),rectangle);
                        g.Save();
                        break;
                    }
                    g.DrawLine(new Pen(Color.Blue, 3), tuple.Item1, tuple.Item2, tuplenew.Item1, tuplenew.Item2);
                    g.Save();
                    tuple = tuplenew;
                    var orientation = Geometriccharacteristics.GetOrientation(labels, i);
                    var RightLeft = GetPrevandNextPoint(labels, tuple.Item1, tuple.Item2, orientation, 1);
                    while (labels[x, y] == 0 || labels[x, y] == i)
                    {
                        var nextPoint = GetNextPoint(x, y, orientation, 10, RightLeft);
                        x = (int)Math.Round(nextPoint.newX);
                        y = (int)Math.Round(nextPoint.newY);
                    }
                    tuplenew = Geometriccharacteristics.GetCentroid(labels, labels[x, y]);
                    i = labels[x, y];
                }
                pictureBox2.Refresh();
                g.Dispose();
            }
            catch
            {
                MessageBox.Show("Выберите изображение");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Сохранить картинку как...";
                sfd.OverwritePrompt = true;
                sfd.CheckPathExists = true;
                sfd.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                sfd.ShowHelp = true;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox2.Image.Save(sfd.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}