using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace SudoMe
{
    public partial class SudoMe : Form
    {
        public TextBox[,] SudokuMatrix;
        byte[,] LoadedBoxes;

        public SudoMe()
        {
            InitializeComponent();
        }

        public bool LoadData()
        {
           LoadedBoxes = new byte[9, 9];
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                {
                    if (SudokuMatrix[y, x].Text == "")
                        LoadedBoxes[y, x] = 0;
                    else
                    {
                        int i;
                        if (int.TryParse(SudokuMatrix[y, x].Text, out i))
                        {
                            if (i > 0 && i <= 9)
                                LoadedBoxes[y, x] = byte.Parse(SudokuMatrix[y, x].Text);
                            else
                                return false;
                        }
                        else
                            return false;
                    }
                }
            return true;
        }

        public void Display(byte[,] SudokuSolved)
        {
            for (int yy = 0; yy < 9; yy++)
                for (int xx = 0; xx < 9; xx++)
                {
                    if (LoadedBoxes[yy, xx] == 0)
                    {
                        SudokuMatrix[yy, xx].Text = SudokuSolved[yy, xx].ToString();
                    }
                    else
                    {
                        SudokuMatrix[yy, xx].BackColor = Color.CornflowerBlue;
                    }
                    SudokuMatrix[yy, xx].ReadOnly = true;
                }
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Solve_Click(object sender, EventArgs e)
        {
            if (!SudokuMatrix[0, 0].ReadOnly)
            {
                if (LoadData())
                {
                    Sudoku S = new Sudoku();
                    byte[,] SudokuSolved = new byte[9, 9];

                    S.Data = new byte[9, 9];
                    for (int y = 0; y < 9; y++)
                        for (int x = 0; x < 9; x++)
                        {
                            S.Data[y, x] = LoadedBoxes[y, x];
                        }

                    if (!S.IsSudokuFeasible())
                    {
                        MessageBox.Show("Sudoku is not feasible!!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ClearBoxes();
                    }
                    else
                        if (S.Solve())
                        {
                            SudokuSolved = S.Data;
                            Display(SudokuSolved);
                        }
                        else
                            MessageBox.Show("Sorry, There is no answer for the Sudoku you have submitted, Please check values", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Check input information for wrong format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearBoxes();
                }
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            ClearBoxes();
        }

        public void ClearBoxes()
        {
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                {
                    SudokuMatrix[y, x].Text = "";
                    SudokuMatrix[y, x].BackColor = Color.White;
                    SudokuMatrix[y, x].ReadOnly = false;
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TextBox TxTBoxes;
            SudokuMatrix = new TextBox[9, 9];
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                {
                    // Draw TextBoxes.
                    TxTBoxes = new TextBox();
                    TxTBoxes.MaxLength = 1;
                    TxTBoxes.Size = new Size(23, 20);
                    TxTBoxes.Font = new Font(new FontFamily("Tahoma"), 8);
                    TxTBoxes.TextAlign = HorizontalAlignment.Center;
                    TxTBoxes.Location = new Point(x * 29 + 44, y * 26 + 70);
                    panel1.Controls.Add(TxTBoxes);
                    SudokuMatrix[y, x] = TxTBoxes;
                }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            System.Drawing.Graphics graphicsObj;

            graphicsObj = e.Graphics;

            Pen myPen = new Pen(System.Drawing.Color.Blue, 3);

            // Draw vertical lines.
            for (int x = 0; x < 2; x++)
                graphicsObj.DrawLine(myPen, x * 87 + 128, 70, x * 87 + 128, 298);

            // Draw horizontal lines.
            for (int x = 0; x < 2; x++)
                graphicsObj.DrawLine(myPen, 43, x * 78 + 145, 299, x * 78 + 145);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Information: SudoMe is an application that solves\nSudoku game using mathematical algorithms\nand techniques.\n---------------------------\nCoded By: Shady Ahmed El-Yaski\nEmail: shady@elyaski.com\nV 1.5.2", "About...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void Print_Click(object sender, EventArgs e)
        {
            if (SudokuMatrix[0, 0].ReadOnly == true)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //new bitmap object to save the image
                    Bitmap bmp = new Bitmap(panel1.Width, panel1.Height);

                    //Drawing control to the bitmap
                    panel1.DrawToBitmap(bmp, new Rectangle(0, 0, panel1.Width, panel1.Height));

                    bmp.Save(saveFileDialog1.FileName);
                    bmp.Dispose();
                }
            }
            else MessageBox.Show("Sudoku is not complete", "Error", MessageBoxButtons.OK, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1);
        }
    }
}