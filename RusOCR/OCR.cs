﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace RusOCR
{
    /// <summary>
    /// Определение текста с картинки
    /// </summary>
    public class Ocr : IDisposable
    {
        #region var

        private List<TextBox> _lines;
        private Image _image;

        #endregion

        #region prop

        public List<TextBox> Lines
        {
            get { return _lines; }
            set { _lines = value; }
        }

        public Image Image
        {
            get { return _image; }
            set { _image = value; }
        }

        #endregion

        #region constr

        public Ocr(System.Drawing.Image image)
        {
            _image = image;
        }

        public Ocr()
        { }
        #endregion

        #region methods

        /// <summary>
        /// Определяет области на которых есть символы
        /// </summary>
        /// <param name="image"></param>
        private void FindTextBox()
        {
            // флаг который показывает есть ли на линии пиксель отличный от белого
            bool flag = false;

            _lines = new List<TextBox>();

            // получаем размер изображения
            var size = Image.Size;

            var btmImage = (Bitmap) Image;

            int upperLeft = 0;
            int lowerLeft = 0;

            for (int y = 0; y < size.Height; y++)
            {
                for (int x = 0; x < size.Width; x++)
                {
                    var pix = btmImage.GetPixel(x, y);

                    // если цвет не белый
                    if (((pix.R + pix.B + pix.G)/3) != 255)
                    {
                        if (flag == false) // если флаг установлен в фолс
                        {
                            flag = true;

                            lowerLeft = y;

                            break;
                        }
                        else // если флаг установлен на тру
                        {
                            break;
                        }
                    }

                    // если последний пиксель белый, значит на линии нет черных, можно отметить как конец линии
                    if (x == size.Width - 1)
                    {
                        // если флаг был помечен как тру
                        if (flag == true)
                        {
                            upperLeft = y - 1;

                            flag = false;

                            _lines.Add(FindColumn(Image, upperLeft, lowerLeft));
                        }
                    }
                }
            }
        }

        private TextBox FindColumn(Image image, int upperLeft, int lowerLeft)
        {
            int xMin = image.Width;
            int xMax = 0;

            var im = (Bitmap) image;

            for (int y = lowerLeft; y < upperLeft; y++)
            {
                // Ищем минимальную точку слева
                for (int x = 0; x < image.Width; x++)
                {
                    var pix = im.GetPixel(x, y);

                    if (((pix.B + pix.G + pix.R)/3) != 255)
                    {
                        if (xMin > x)
                        {
                            xMin = x;
                        }
                    }
                }

                // ищем максимальную точку справа
                for (int x = image.Width - 1; x > 0; x--)
                {
                    var pix = im.GetPixel(x, y);

                    if (((pix.B + pix.G + pix.R) / 3) != 255)
                    {
                        if (xMax < x)
                        {
                            xMax = x;
                        }
                    }
                }
            }

            return new TextBox() {UpperLeft = upperLeft, LowerLeft = lowerLeft, UpperRight = xMax, LowerRight = xMin};
        }

        public void PrintImage()
        {
            if (_image == null)
            {
                throw new NullReferenceException(nameof(_image));
            }

            (_image as Bitmap).Save(@"result/test.png", ImageFormat.Png);
        }

        public void FindKeyA()
        {
            if (_image == null)
            {
                throw new NullReferenceException(nameof(_image));
            }

            FindTextBox();

            WashTextBox();

            CuteFirstSymbol();
        }

        private void CuteFirstSymbol()
        {
            
        }

        private void WashTextBox()
        {
            var image = (Bitmap) _image;

            foreach (var textBox in _lines)
            {
                for (int i = textBox.LowerLeft; i < textBox.UpperLeft; i++)
                {
                    for (int j = textBox.LowerRight; j < textBox.UpperRight; j++)
                    {
                        var pix = image.GetPixel(j, i);
                        int sum = ((pix.R + pix.B + pix.G) / 3);

                        if (sum > 150)
                        {
                            image.SetPixel(j,i,Color.FromArgb(255,255,255));
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Image = null;
        }

        #endregion
    }
}