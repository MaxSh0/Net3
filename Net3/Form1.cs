using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace Net3
{
    public partial class Form1 : Form
    {
        public static Bitmap image;
        public static string full_name_of_image = "\0";
        public static UInt32[,] pixel;
        //
        //
        //HABR
        private BitArray ByteToBit(byte src)//байт в бит
        {
            BitArray bitArray = new BitArray(8);
            bool st = false;
            for (int i = 0; i < 8; i++)
            {
                if ((src >> i & 1) == 1)
                {
                    st = true;
                }
                else st = false;
                bitArray[i] = st;
            }
            return bitArray;
        }

        private byte BitToByte(BitArray scr)//бит в байт
        {
            byte num = 0;
            for (int i = 0; i < scr.Count; i++)
                if (scr[i] == true)
                    num += (byte)Math.Pow(2, i);
            return num;
        }

        /*Проверяет, зашифрован ли файл,  возвраещает true, если символ в первом пикслеле равен / иначе false */
        private bool isEncryption(Bitmap scr)
        {
            byte[] rez = new byte[1];
            Color color = scr.GetPixel(0, 0);
            BitArray colorArray = ByteToBit(color.R); //получаем байт цвета и преобразуем в массив бит
            BitArray messageArray = ByteToBit(color.R); ;//инициализируем результирующий массив бит
            messageArray[0] = colorArray[0];
            messageArray[1] = colorArray[1];

            colorArray = ByteToBit(color.G);//получаем байт цвета и преобразуем в массив бит
            messageArray[2] = colorArray[0];
            messageArray[3] = colorArray[1];
            messageArray[4] = colorArray[2];

            colorArray = ByteToBit(color.B);//получаем байт цвета и преобразуем в массив бит
            messageArray[5] = colorArray[0];
            messageArray[6] = colorArray[1];
            messageArray[7] = colorArray[2];
            rez[0] = BitToByte(messageArray); //получаем байт символа, записанного в 1 пикселе
            string m = Encoding.GetEncoding(1251).GetString(rez);
            if (m == "/")
            {
                return true;
            }
            else return false;
        }

        /*Записыает количество символов для шифрования в первые биты картинки */
        private void WriteCountText(int count, Bitmap src)
        {
            byte[] CountSymbols = Encoding.GetEncoding(1251).GetBytes(count.ToString());
            for (int i = 0; i < 3; i++)
            {
                BitArray bitCount = ByteToBit(CountSymbols[i]); //биты количества символов
                Color pColor = src.GetPixel(0, i + 1); //1, 2, 3 пикселы
                BitArray bitsCurColor = ByteToBit(pColor.R); //бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[0];
                bitsCurColor[1] = bitCount[1];
                byte nR = BitToByte(bitsCurColor); //новый бит цвета пиксея

                bitsCurColor = ByteToBit(pColor.G);//бит бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[2];
                bitsCurColor[1] = bitCount[3];
                bitsCurColor[2] = bitCount[4];
                byte nG = BitToByte(bitsCurColor);//новый цвет пиксея

                bitsCurColor = ByteToBit(pColor.B);//бит бит цветов текущего пикселя
                bitsCurColor[0] = bitCount[5];
                bitsCurColor[1] = bitCount[6];
                bitsCurColor[2] = bitCount[7];
                byte nB = BitToByte(bitsCurColor);//новый цвет пиксея

                Color nColor = Color.FromArgb(nR, nG, nB); //новый цвет из полученных битов
                src.SetPixel(0, i + 1, nColor); //записали полученный цвет в картинку
            }
        }

        /*Читает количество символов для дешифрования из первых бит картинки*/
        private int ReadCountText(Bitmap src)
        {
            byte[] rez = new byte[3]; //массив на 3 элемента, т.е. максимум 999 символов шифруется
            for (int i = 0; i < 3; i++)
            {
                Color color = src.GetPixel(0, i + 1); //цвет 1, 2, 3 пикселей 
                BitArray colorArray = ByteToBit(color.R); //биты цвета
                BitArray bitCount = ByteToBit(color.R); ; //инициализация результирующего массива бит
                bitCount[0] = colorArray[0];
                bitCount[1] = colorArray[1];

                colorArray = ByteToBit(color.G);
                bitCount[2] = colorArray[0];
                bitCount[3] = colorArray[1];
                bitCount[4] = colorArray[2];

                colorArray = ByteToBit(color.B);
                bitCount[5] = colorArray[0];
                bitCount[6] = colorArray[1];
                bitCount[7] = colorArray[2];
                rez[i] = BitToByte(bitCount);
            }
            string m = Encoding.GetEncoding(1251).GetString(rez);
            return Convert.ToInt32(m, 10);
        }
        //
        //
        //
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;

            if (radioButton1.Checked)
            {
                
                string FilePic;
                string FileText;
                if (openFileDialogIMG.ShowDialog() == DialogResult.OK)
                {
                    FilePic = openFileDialogIMG.FileName;
                }
                else
                {
                    FilePic = "";
                    return;
                }

                FileStream rFile;
                try
                {
                    rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Bitmap bPic = new Bitmap(rFile);
                Bitmap FirstPic = new Bitmap(rFile);

                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;//растягивает изображение на PictureBox
                pictureBox1.Image = bPic;//устанавливает изображение в форму

                if (openFileDialogTEXT.ShowDialog() == DialogResult.OK)
                {
                    FileText = openFileDialogTEXT.FileName;
                }
                else
                {
                    FileText = "";
                    return;
                }
                FileStream rText;
                try
                {
                    rText = new FileStream(FileText, FileMode.Open); //открываем поток
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                StreamReader str = new StreamReader(rText);
                textBox1.Text = str.ReadToEnd();
                str.Dispose();

                try
                {
                    rText = new FileStream(FileText, FileMode.Open); //открываем поток
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                BinaryReader bText = new BinaryReader(rText, Encoding.ASCII);

                List<byte> bList = new List<byte>();
                while (bText.PeekChar() != -1)
                { //считали весь текстовый файл для шифрования в лист байт
                    bList.Add(bText.ReadByte());
                }
                int CountText = bList.Count; // в CountText - количество в байтах текста, который нужно закодировать
                bText.Close();
                rFile.Close();

                //проверяем, поместиться ли исходный текст в картинке
                if (CountText > ((bPic.Width * bPic.Height)) - 4)
                {
                    MessageBox.Show("Выбранная картинка мала для размещения выбранного текста", "Информация", MessageBoxButtons.OK);
                    return;
                }

                //проверяем, может быть картинка уже зашифрована
                if (isEncryption(bPic))
                {
                    MessageBox.Show("Файл уже зашифрован", "Информация", MessageBoxButtons.OK);
                    return;
                }

                byte[] Symbol = Encoding.GetEncoding(1251).GetBytes("/");
                BitArray ArrBeginSymbol = ByteToBit(Symbol[0]);
                Color curColor = bPic.GetPixel(0, 0);
                BitArray tempArray = ByteToBit(curColor.R);
                tempArray[0] = ArrBeginSymbol[0];
                tempArray[1] = ArrBeginSymbol[1];
                byte nR = BitToByte(tempArray);

                tempArray = ByteToBit(curColor.G);
                tempArray[0] = ArrBeginSymbol[2];
                tempArray[1] = ArrBeginSymbol[3];
                tempArray[2] = ArrBeginSymbol[4];
                byte nG = BitToByte(tempArray);

                tempArray = ByteToBit(curColor.B);
                tempArray[0] = ArrBeginSymbol[5];
                tempArray[1] = ArrBeginSymbol[6];
                tempArray[2] = ArrBeginSymbol[7];
                byte nB = BitToByte(tempArray);

                Color nColor = Color.FromArgb(nR, nG, nB);
                bPic.SetPixel(0, 0, nColor);
                //то есть в первом пикселе будет символ /, который говорит о том, что картика зашифрована

                WriteCountText(CountText, bPic); //записываем количество символов для шифрования

                int index = 0;
                bool st = false;
                for (int i = 4; i < bPic.Width; i++)
                {
                    for (int j = 0; j < bPic.Height; j++)
                    {
                        Color pixelColor = bPic.GetPixel(i, j);
                        if (index == bList.Count)
                        {
                            st = true;
                            break;
                        }
                        BitArray colorArray = ByteToBit(pixelColor.R);
                        BitArray messageArray = ByteToBit(bList[index]);
                        colorArray[0] = messageArray[0]; //меняем
                        colorArray[1] = messageArray[1]; // в нашем цвете биты
                        byte newR = BitToByte(colorArray);

                        colorArray = ByteToBit(pixelColor.G);
                        colorArray[0] = messageArray[2];
                        colorArray[1] = messageArray[3];
                        colorArray[2] = messageArray[4];
                        byte newG = BitToByte(colorArray);

                        colorArray = ByteToBit(pixelColor.B);
                        colorArray[0] = messageArray[5];
                        colorArray[1] = messageArray[6];
                        colorArray[2] = messageArray[7];
                        byte newB = BitToByte(colorArray);

                        Color newColor = Color.FromArgb(newR, newG, newB);
                        bPic.SetPixel(i, j, newColor);
                        index++;
                    }
                    if (st)
                    {
                        break;
                    }
                }
                pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;//растягивает изображение на PictureBox
                pictureBox2.Image = bPic;

                String sFilePic;
                if (saveFileDialogIMG.ShowDialog() == DialogResult.OK)
                {
                    sFilePic = saveFileDialogIMG.FileName;
                }
                else
                {
                    sFilePic = "";
                    return;
                };


                FileStream wFile;
                try
                {
                    wFile = new FileStream(sFilePic, FileMode.Create); //открываем поток на запись результатов
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bPic.Save(wFile, System.Drawing.Imaging.ImageFormat.Bmp);
                wFile.Close(); //закрываем поток
                for (int i = 0; i < FirstPic.Width; i++)
                {
                    for (int j = 0; j < FirstPic.Height; j++)
                    {
                        Color newColor;
                        Color pixelColor = bPic.GetPixel(i, j);
                        Color pixelColor2 = FirstPic.GetPixel(i, j);
                        BitArray colorArray = ByteToBit(pixelColor.R);
                        BitArray colorArray2 = ByteToBit(pixelColor2.R);
                        BitArray colorArray3 = ByteToBit(pixelColor.B);
                        BitArray colorArray4 = ByteToBit(pixelColor2.B);
                        BitArray colorArray5 = ByteToBit(pixelColor.G);
                        BitArray colorArray6 = ByteToBit(pixelColor2.G);
                        if (colorArray[0] != colorArray2[0] || colorArray[1] != colorArray2[1] || colorArray[2] != colorArray2[2])
                        {
                            newColor = Color.White;
                        }
                        else if (colorArray3[0] != colorArray4[0] || colorArray3[1] != colorArray4[1] || colorArray3[2] != colorArray4[2])
                        {
                            newColor = Color.Violet;
                        }
                        else if (colorArray5[0] != colorArray6[0] || colorArray5[1] != colorArray6[1] || colorArray5[2] != colorArray6[2])
                        {
                            newColor = Color.Red;
                        }
                        else
                        {
                            newColor = Color.FromArgb(pixelColor.ToArgb() - pixelColor2.ToArgb());
                        }
                        bPic.SetPixel(i, j, newColor);
                        index++;
                    }
                }
                pictureBox3.Image = bPic;

                if (checkBox2.Checked)
                {
                    
                    if (saveFileDialogIMG.ShowDialog() == DialogResult.OK)
                    {
                        sFilePic = saveFileDialogIMG.FileName;
                    }
                    else
                    {
                        sFilePic = "";
                        return;
                    };

                    FileStream sFile;
                    try
                    {
                        sFile = new FileStream(sFilePic, FileMode.Create); //открываем поток на запись результатов
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    bPic.Save(sFile, System.Drawing.Imaging.ImageFormat.Bmp);
                    sFile.Close(); //закрываем поток
                }
                
               
            }
            else if(radioButton2.Checked)
            {
                string FilePic;
                if (openFileDialogIMG.ShowDialog() == DialogResult.OK)
                {
                    FilePic = openFileDialogIMG.FileName;
                }
                else
                {
                    FilePic = "";
                    return;
                }

                FileStream rFile;
                try
                {
                    rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Bitmap bPic = new Bitmap(rFile);
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;//растягивает изображение на PictureBox
                pictureBox1.Image = bPic;
                if (!isEncryption(bPic))
                {
                    MessageBox.Show("В файле нет зашифрованной информации", "Информация", MessageBoxButtons.OK);
                    return;
                }

                int countSymbol = ReadCountText(bPic); //считали количество зашифрованных символов
                byte[] message = new byte[countSymbol];
                int index = 0;
                bool st = false;
                for (int i = 4; i < bPic.Width; i++)
                {
                    for (int j = 0; j < bPic.Height; j++)
                    {
                        Color pixelColor = bPic.GetPixel(i, j);
                        if (index == message.Length)
                        {
                            st = true;
                            break;
                        }
                        BitArray colorArray = ByteToBit(pixelColor.R);
                        BitArray messageArray = ByteToBit(pixelColor.R); ;
                        messageArray[0] = colorArray[0];
                        messageArray[1] = colorArray[1];

                        colorArray = ByteToBit(pixelColor.G);
                        messageArray[2] = colorArray[0];
                        messageArray[3] = colorArray[1];
                        messageArray[4] = colorArray[2];

                        colorArray = ByteToBit(pixelColor.B);
                        messageArray[5] = colorArray[0];
                        messageArray[6] = colorArray[1];
                        messageArray[7] = colorArray[2];
                        message[index] = BitToByte(messageArray);
                        index++;
                    }
                    if (st)
                    {
                        break;
                    }
                }
                string strMessage = Encoding.GetEncoding(1251).GetString(message);
                
                textBox1.Text = strMessage;
                if (checkBox1.Checked)
                {
                    string sFileText;
                    if (saveFileDialogTEXT.ShowDialog() == DialogResult.OK)
                    {
                        sFileText = saveFileDialogTEXT.FileName;
                    }
                    else
                    {
                        sFileText = "";
                        return;
                    };

                    FileStream wFile;
                    try
                    {
                        wFile = new FileStream(sFileText, FileMode.Create); //открываем поток на запись результатов
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    StreamWriter wText = new StreamWriter(wFile, Encoding.Default);
                    wText.Write(strMessage);
                    MessageBox.Show("Текст записан в файл", "Информация", MessageBoxButtons.OK);
                    wText.Close();
                    wFile.Close(); //закрываем поток
                }



            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioB();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioB();
        }

        private void radioB()
        {
            if (radioButton1.Checked)
            {
                button1.Text = "Зашифровать";
                checkBox1.Visible = false;
                checkBox2.Visible = true;
            }
            else if (radioButton2.Checked)
            {
                button1.Text = "Расшифровать";
                checkBox1.Visible = true;
                checkBox2.Visible = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Для изменения яркости на N процентов используется следующая формула:
            //I = I + N • 128 / 100

                string FilePic;
                if (openFileDialogIMG.ShowDialog() == DialogResult.OK)
                {
                    FilePic = openFileDialogIMG.FileName;
                }
                else
                {
                    FilePic = "";
                    return;
                }

                FileStream rFile;
                try
                {
                    rFile = new FileStream(FilePic, FileMode.Open); //открываем поток
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка открытия файла", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                image = new Bitmap(rFile);
                Bitmap imageOriginal = new Bitmap(rFile);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = imageOriginal;
            pixel = new UInt32[image.Height, image.Width];
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    pixel[y, x] = (UInt32)(image.GetPixel(x, y).ToArgb());
                }
            }

            UInt32 p;
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    p = Brightness(pixel[i, j], 5, 10);
                    FromOnePixelToBitmap(i, j, p);
                }
            }
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = image;




            String sFilePic;
            if (saveFileDialogIMG.ShowDialog() == DialogResult.OK)
            {
                sFilePic = saveFileDialogIMG.FileName;
            }
            else
            {
                sFilePic = "";
                return;
            };


            FileStream wFile;
            try
            {
                wFile = new FileStream(sFilePic, FileMode.Create); //открываем поток на запись результатов
            }
            catch (IOException)
            {
                MessageBox.Show("Ошибка открытия файла на запись", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            image.Save(wFile, System.Drawing.Imaging.ImageFormat.Bmp);
            wFile.Close(); //закрываем поток
        }

        public static void FromOnePixelToBitmap(int x, int y, UInt32 pixel)
        {
            image.SetPixel(y, x, Color.FromArgb((int)pixel));
        }


        public UInt32 Brightness(UInt32 point, int poz, int lenght)
        {
            int R;
            int G;
            int B;

            int N = (100 / lenght) * poz; //кол-во процентов

            R = (int)(((point & 0x00FF0000) >> 16) + N * 128 / 100);
            G = (int)(((point & 0x0000FF00) >> 8) + N * 128 / 100);
            B = (int)((point & 0x000000FF) + N * 128 / 100);

            //контролируем переполнение переменных
            if (R < 0) R = 0;
            if (R > 255) R = 255;
            if (G < 0) G = 0;
            if (G > 255) G = 255;
            if (B < 0) B = 0;
            if (B > 255) B = 255;

            point = 0xFF000000 | ((UInt32)R << 16) | ((UInt32)G << 8) | ((UInt32)B);

            return point;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
        }
    }
}






