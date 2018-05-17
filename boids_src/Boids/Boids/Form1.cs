using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Boids
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private Swarm swarm;
        private Image iconRegular;
        private Image iconZombie;
        public Form1()
        {
            int boundary = 640;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(boundary, boundary);
            iconRegular = CreateIcon(Brushes.Blue);
            iconZombie = CreateIcon(Brushes.Red);
            swarm = new Swarm(boundary);
            timer = new Timer();
            timer.Tick += new EventHandler(this.timer_Tick);
            timer.Interval = 75;
            timer.Start();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (Boid boid in swarm.Boids)
            {
                float angle;
                if (boid.dX == 0) angle = 90f;
                else angle = (float)(Math.Atan(boid.dY / boid.dX) * 57.3);
                if (boid.dX < 0f) angle += 180f;
                Matrix matrix = new Matrix();
                matrix.RotateAt(angle, boid.Position);
                e.Graphics.Transform = matrix;
                if (boid.Zombie) e.Graphics.DrawImage(iconZombie, boid.Position);
                else e.Graphics.DrawImage(iconRegular, boid.Position);
            }
        }

        private static Image CreateIcon(Brush brush)
        {
            Bitmap icon = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(icon);
            Point p1 = new Point(0, 16);
            Point p2 = new Point(16, 8);
            Point p3 = new Point(0, 0);
            Point p4 = new Point(5, 8);
            Point[] points = { p1, p2, p3, p4 };
            g.FillPolygon(brush, points);
            return icon;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            swarm.MoveBoids();
            Invalidate();
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                swarm.Boids.Add(new Boid(true, 640, e.Location));
            else if (e.Button == MouseButtons.Right)
                swarm.Boids.Add(new Boid(false, 640, e.Location));
            
            if (e.Button == MouseButtons.Middle)
                midPressed = true;
                
        }
        static public Point curMouse = new Point();
        static public bool midPressed = false;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
                midPressed = false;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            curMouse = e.Location;
        }
    }

    public class Swarm
    {
        public List<Boid> Boids = new List<Boid>();

        public Swarm(int boundary)
        {
            for (int i = 0; i < 15; i++)
            {
                Boids.Add(new Boid(false, boundary));
            }
        }

        public void MoveBoids()
        {
            foreach (Boid boid in Boids)
            {
                boid.Move(Boids);
            }
        }
    }

    public class Boid
    {
        private static Random rnd = new Random();
        private static float border = 100f;
        private static float sight = 75f;
        private static float space = 30f;
        private static float speed = 12f;
        private float boundary;
        public float dX = 8f;
        public float dY = 8f;
        public bool Zombie;
        public PointF Position;

        public Boid(bool zombie, int boundary)
        {
            Position = new PointF(rnd.Next(boundary), rnd.Next(boundary));
            this.boundary = boundary;
            Zombie = zombie;
        }
        public Boid(bool zombie, int boundary, Point p)
        {
            Position = new PointF(p.X, p.Y);
            this.boundary = boundary;
            Zombie = zombie;
        }

        public void Move(List<Boid> boids)
        {
            if (!Zombie) Flock(boids);
            else Hunt(boids);
            CheckBounds();
            CheckSpeed();
            Position.X += dX;
            Position.Y += dY;
        }

        private void Flock(List<Boid> boids)
        {
            foreach (Boid boid in boids)
            {
                float distance = Distance(Position, boid.Position);
                if (boid != this && !boid.Zombie)
                {
                    if (distance < space)
                    {
                        // Create space.
                        dX += Position.X - boid.Position.X;
                        dY += Position.Y - boid.Position.Y;
                    }
                    else if (distance < sight)
                    {
                        // Flock together.
                        dX += (boid.Position.X - Position.X) * 0.05f;
                        dY += (boid.Position.Y - Position.Y) * 0.05f;

                         // Align movement.
                        dX += boid.dX * 0.5f;
                        dY += boid.dY * 0.5f;
                    }
                    if (distance < sight)
                    {
                       
                    }

                }
                if (boid.Zombie && distance < sight)
                {
                    // Avoid zombies.
                    dX += Position.X - boid.Position.X;
                    dY += Position.Y - boid.Position.Y;
                }
                if (Form1.midPressed)
                {
                    dX -= Position.X - Form1.curMouse.X;
                    dY -= Position.Y - Form1.curMouse.Y;
                }
            }
        }

        private void Hunt(List<Boid> boids)
        {
            float range = float.MaxValue;
            Boid prey = null;
            foreach (Boid boid in boids)
            {
                if (!boid.Zombie)
                {
                    float distance = Distance(Position, boid.Position);
                    if (distance < sight && distance < range)
                    {
                        range = distance;
                        prey = boid;
                    }
                }
            }
            if (prey != null)
            {
                // Move towards closest prey.
                dX += prey.Position.X - Position.X;
                dY += prey.Position.Y - Position.Y;
            }
        }

        private static float Distance(PointF p1, PointF p2)
        {
            double val = Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);
            return (float)Math.Sqrt(val);
        }

        private void CheckBounds()
        {
            float val = boundary - border;
            if (Position.X < border) dX += border - Position.X;
            if (Position.Y < border) dY += border - Position.Y;
            if (Position.X > val) dX += val - Position.X;
            if (Position.Y > val) dY += val - Position.Y;
        }

        private void CheckSpeed()
        {
            float s;
            if (!Zombie) s = speed;
            else s = speed / 4f;
            float val = Distance(new PointF(0f, 0f), new PointF(dX, dY));
            if (val > s)
            {
                dX = dX * s / val;
                dY = dY * s / val;
            }
        }
    }
}
