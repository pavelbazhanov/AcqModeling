using System;

namespace AcqModeling
{
    public enum stat { Scattered, Free, Absorbed, Finished };
    public class Photon
    {
        private static Random random = new Random();
        // Начальные координаты фотона
        public Vector3d StartPosition;
        // Координаты фотона (мировые в мм)
        public Vector3d CurrentPosition;

        // Направление движения (углы)
        public double Phi; // Azimuthal angle
        public double Theta; // Elevation angle

        public stat Status;
        public double Energy;
        public double Path;

        public bool WasScattered = false;

        // Время образования (нс)
        public double Time;

        // для статитики определяем кол-во рассеиваний по пути до детектора
        public int numScatterEvents;

        public Photon(AnnihilationEvent ev, bool inverse = false)
        {
            this.numScatterEvents = 0;

            this.StartPosition = this.CurrentPosition = ev.Position;

            if (inverse)
            {
                this.Phi = ev.Phi + Math.PI;
                this.Theta = -ev.Theta;
                if (this.Phi > 2 * Math.PI)
                    this.Phi -= 2 * Math.PI;
            }
            else
            {
                this.Phi = ev.Phi;
                this.Theta = ev.Theta;
            }

            this.Status = stat.Free;
            this.Time = ev.Time;
            this.Energy = ev.Energy;
            this.Path = 0;
        }
        //public void Trace(Phantom ph, double cutOffPercent)
        //{
        //    do
        //    {
        //        if (this.Status == stat.Scattered)
        //        {
        //            double angle_phi, angle_theta; // новые направляющие углы
        //            //angle_phi = Math.Acos(2 * random.NextDouble() - 1);
        //            //angle_theta = 2 * Math.PI * random.NextDouble();
        //            //double scat_angle = Math.Acos(Math.Sin(this.Theta) * Math.Sin(angle_theta) * Math.Cos(this.Phi - angle_phi) + Math.Cos(this.Theta) * Math.Cos(angle_theta));

        //            angle_phi = 2 * Math.PI * random.NextDouble();
        //            angle_theta = Math.Acos(2 * random.NextDouble() - 1);
        //            Vector3d A= new Vector3d(Math.Sin(this.Theta)*Math.Cos(this.Phi),Math.Sin(this.Theta)*Math.Sin(this.Phi), Math.Cos(this.Theta) ), 
        //                B = new Vector3d(Math.Sin(angle_theta)*Math.Cos(angle_phi),Math.Sin(angle_theta)*Math.Sin(angle_phi), Math.Cos(angle_theta));
        //            double scat_angle = Math.Acos(A*B/(A.Length * B.Length));

        //            this.Energy = this.Energy / (2 - Math.Cos(scat_angle));
        //            this.Phi = angle_phi;
        //            this.Theta = angle_theta;

        //            this.numScatterEvents++;
        //        }
        //        if (this.Energy < cutOffPercent * 511)
        //        {
        //            this.Status = stat.Absorbed;
        //            break;
        //        }
        //        double ksi;// свободный пробег для фотона в мм
        //        if (ph.Alpha == 0)
        //        {
        //            this.Status = stat.Finished;
        //            break;
        //        }
        //        else
        //            ksi = -Math.Log(random.NextDouble()) / ph.Alpha;

        //        this.Path += ksi;
        //        // Новые координаты фотонов
        //        this.StartPosition = this.CurrentPosition;

        //        this.CurrentPosition = this.CurrentPosition + ksi * new Vector3d(Math.Cos(this.Phi) * Math.Cos(this.Theta), Math.Sin(this.Phi) * Math.Cos(this.Theta), Math.Sin(this.Theta));
        //        // Проверка того, не вышел ли фотон за пределы области, если не вышел, то моделируем его "судьбу"
        //        if (!Utils.IsBelong(ph, this.CurrentPosition))
        //            this.Status = stat.Finished;
        //        else
        //            ChooseDestiny(ph, ph.Alpha);
        //    }
        //    while (this.Status != stat.Finished && this.Status != stat.Absorbed);
        //}
        public void Trace(double rad, double length, double mus, double mue, double cutOffPercent)
        {
            do
            {
                if (this.Status == stat.Scattered)
                {
                    double angle_phi, angle_theta; // новые направляющие углы
                    //angle_phi = Math.Acos(2 * random.NextDouble() - 1);
                    //angle_theta = 2 * Math.PI * random.NextDouble();
                    //double scat_angle = Math.Acos(Math.Sin(this.Theta) * Math.Sin(angle_theta) * Math.Cos(this.Phi - angle_phi) + Math.Cos(this.Theta) * Math.Cos(angle_theta));

                    angle_phi = 2 * Math.PI * random.NextDouble();
                    angle_theta = Math.Acos(2 * random.NextDouble() - 1);

                    Vector3d A = new Vector3d(Math.Sin(this.Theta) * Math.Cos(this.Phi), Math.Sin(this.Theta) * Math.Sin(this.Phi), Math.Cos(this.Theta)),
                        B = new Vector3d(Math.Sin(angle_theta) * Math.Cos(angle_phi), Math.Sin(angle_theta) * Math.Sin(angle_phi), Math.Cos(angle_theta));
                    double scat_angle = Math.Acos(A * B / (A.Length * B.Length));
                    this.Energy = this.Energy / (2 - Math.Cos(scat_angle));

                    this.Phi = angle_phi;
                    this.Theta = angle_theta;

                    this.numScatterEvents++;
                }
                if (this.Energy < cutOffPercent * 511E3)
                {
                    this.Status = stat.Absorbed;
                    break;
                }

                if (mus + mue == 0)
                {
                    this.Status = stat.Finished;
                    break;
                }

                double ksi = -Math.Log(random.NextDouble()) / (mus + mue); // свободный пробег для фотона в мм
                    
                this.Path += ksi;
                // Новые координаты фотонов
                this.StartPosition = this.CurrentPosition;

                this.CurrentPosition = this.CurrentPosition + ksi * new Vector3d(Math.Cos(this.Phi) * Math.Cos(this.Theta), Math.Sin(this.Phi) * Math.Cos(this.Theta), Math.Sin(this.Theta));
                // Проверка того, не вышел ли фотон за пределы области, если не вышел, то моделируем его "судьбу"
                if ((this.CurrentPosition.X * this.CurrentPosition.X + this.CurrentPosition.Y * this.CurrentPosition.Y) >= rad * rad ||
                    this.CurrentPosition.Z <= -length / 2 && this.CurrentPosition.Z >= length / 2)
                    this.Status = stat.Finished;
                else
                {
                    double gamma = random.NextDouble();
                    if (gamma < mue / (mue + mus))
                        this.Status = stat.Absorbed;
                    else
                    {
                        this.Status = stat.Scattered;
                        this.WasScattered = true;
                    }
                }
            }
            while (this.Status != stat.Finished && this.Status != stat.Absorbed);
        }
        public void Trace_with_roulette(double rad, double length, double mue, double mus, double chanceToSurvive)
        {
            do
            {
                //direction cosines:
                Vector3d direction = new Vector3d(Math.Cos(Theta) * Math.Cos(Phi), Math.Cos(Theta) * Math.Sin(Phi), Math.Sin(Theta));
                double step = -Math.Log(random.NextDouble()) / (mue + mus);

                CurrentPosition = CurrentPosition + step * direction;

                if ((this.CurrentPosition.X * this.CurrentPosition.X + this.CurrentPosition.Y * this.CurrentPosition.Y) >= rad * rad &&
                    this.CurrentPosition.Z <= -length / 2 && this.CurrentPosition.Z >= length / 2)
                {
                    Status = stat.Finished;
                    break;
                }

                Status = stat.Scattered;

                double dW = (mue / mus) * Energy;
                Energy -= dW;
                double scatAngleTheta = Math.Acos(1 - 2 * random.NextDouble());
                double scatAnglePhi = 2 * Math.PI * random.NextDouble();

                var tmp = direction;
                if (tmp.Z == 1)
                {
                    direction = new Vector3d(Math.Sin(scatAngleTheta) * Math.Cos(scatAnglePhi), Math.Sin(scatAngleTheta) * Math.Sin(scatAnglePhi), Math.Cos(scatAngleTheta));
                }
                else if (tmp.Z == -1)
                {
                    direction = new Vector3d(Math.Sin(scatAngleTheta) * Math.Cos(scatAnglePhi), -Math.Sin(scatAngleTheta) * Math.Sin(scatAnglePhi), -Math.Cos(scatAngleTheta));
                }
                else
                {
                    direction.X = Math.Sin(scatAngleTheta) * (tmp.X * tmp.Z * Math.Cos(scatAnglePhi) - tmp.Y * Math.Sin(scatAnglePhi)) / Math.Sqrt(1 - tmp.Z * tmp.Z) + tmp.X * Math.Cos(scatAngleTheta);
                    direction.Y = Math.Sin(scatAngleTheta) * (tmp.Y * tmp.Z * Math.Cos(scatAnglePhi) + tmp.X * Math.Sin(scatAnglePhi)) / Math.Sqrt(1 - tmp.Z * tmp.Z) + tmp.Y * Math.Cos(scatAngleTheta);
                    direction.Z = -Math.Sqrt(1 - tmp.Z * tmp.Z) * Math.Sin(scatAngleTheta) * Math.Cos(scatAnglePhi) + tmp.Z * Math.Cos(scatAngleTheta);
                }

                if (random.NextDouble() <= chanceToSurvive)
                    Energy = Energy / chanceToSurvive;
                else
                {
                    Energy = 0;
                    Status = stat.Absorbed;
                }
            }
            while (Status != stat.Absorbed);
        }
        //public void Trace_with_roulette(Phantom ph, double cut_off, double m)
        //{//http://en.wikipedia.org/wiki/Monte_Carlo_method_for_photon_transport
            //bool chance_to_survive = true;
            //double mux = StaticRandom.Rand(), muy = StaticRandom.Rand(), muz = StaticRandom.Rand();

            //if (ph.Alpha == 0)
            //{
            //    this.status = stat.Finished;
            //    return;
            //}
            //else
            //do
            //{
            //    int w, h, d;
            //    getVoxelFromCoords(ph, out w, out h, out d);

            //    // свободный пробег для фотона в мм
            //    double ksi;
            //    if (ph.mu_e(w, h, d) + ph.mu_s(w, h, d) != 0)
            //    {
            //        ksi = -Math.Log(StaticRandom.Rand()) / (ph.mu_e(w, h, d) + ph.mu_s(w, h, d));
            //        this.numScatterEvents++;

            //        // Новые координаты фотонов
            //        this.startX = this.x;
            //        this.startY = this.y;
            //        this.startZ = this.z;

            //        this.x = this.x + ksi * mux;
            //        this.y = this.y + ksi * muy;
            //        this.z = this.z + ksi * muz;
            //        path += ksi;

            //        //Новые направляющие углы
            //        this.phi = 2 * Math.PI * StaticRandom.Rand();
            //        this.theta = Math.Acos(1 - 2 * StaticRandom.Rand());

            //        //Находим новые направляющие косинусы
            //        double oldmux = mux, oldmuy = muy, oldmuz = muz;
            //        if (Math.Abs(muz) == 1)
            //        {
            //            mux = Math.Sin(this.theta) * Math.Cos(this.phi);
            //            muy = Math.Sign(muz) * Math.Sin(this.theta) * Math.Sin(this.phi);
            //            muz = Math.Sign(muz) * Math.Cos(this.theta);
            //        }
            //        else
            //        {
            //            mux = Math.Sin(this.theta) * (oldmux * oldmuz * Math.Cos(this.phi) - oldmuy * Math.Sin(this.phi)) / Math.Sqrt(1 - oldmuz * oldmuz) + oldmux * Math.Cos(this.theta);
            //            muy = Math.Sin(this.theta) * (oldmuy * oldmuz * Math.Cos(this.phi) + oldmux * Math.Sin(this.phi)) / Math.Sqrt(1 - oldmuz * oldmuz) + oldmuy * Math.Cos(this.theta);
            //            muz = -Math.Sqrt(1 - oldmuz * oldmuz) * Math.Sin(this.theta) * Math.Cos(this.phi) + oldmuz * Math.Cos(theta);
            //        }

            //        //Меняем энергию фотона
            //        this.energy -= ph.mu_e(w, h, d) / (ph.mu_e(w, h, d) + ph.mu_s(w, h, d)) * this.energy;

            //        //Roulette
            //        if (this.energy < cut_off)
            //            if (chance_to_survive)
            //                //if (random.NextDouble() <= (1 / m))
            //                if (StaticRandom.Rand() < m)
            //                    this.energy = (1 / m) * this.energy;
            //                else
            //                    this.status = stat.Absorbed;
            //    }
            //    else
            //    {
            //        ksi = -Math.Log(StaticRandom.Rand()) / ph.Alpha;
            //        path += ksi;
            //        // Новые координаты фотонов
            //        this.startX = this.x;
            //        this.startY = this.y;
            //        this.startZ = this.z;

            //        this.x = this.x + ksi * mux;
            //        this.y = this.y + ksi * muy;
            //        this.z = this.z + ksi * muz;
            //    }

            //    // Проверка того, не вышел ли фотон за пределы области
            //    if (!ph.Belong(this.x, this.y, this.z))
            //        this.status = stat.Finished;
            //}
            //while (this.status != stat.Finished && this.status != stat.Absorbed);
        //}
        //public void ChooseDestiny(Phantom ph, double alpha)
        //{
        //    var tmp = Utils.WorldToVolume(this.CurrentPosition, ph.Activity);
        //    int w = (int)tmp.X, h = (int)tmp.Y, d = (int)tmp.Z;

        //    double gamma = random.NextDouble();
        //    if (ph.Mu_s(w, h, d) / alpha > gamma)
        //    {
        //        this.Status = stat.Scattered;
        //        this.WasScattered = true;
        //    }
        //    else if (gamma < (ph.Mu_s(w, h, d) + ph.Mu_e(w, h, d)) / alpha)
        //        this.Status = stat.Absorbed;
        //    else
        //    {
        //        this.Status = stat.Free;
        //    }
        //}
    }
}
