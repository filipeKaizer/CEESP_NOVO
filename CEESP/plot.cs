﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CEESP
{
    class plot
    {
        private float centerX;
        private float centerY;
        private float XSValue;

        private float FinalX;
        private float FinalY;

        private bool autoHeigth;

        private SolidColorBrush ActualBrush;
        public plot(float centerXv, float centerYv, float Xs)
        {
            this.centerX = centerXv;
            this.centerY = centerYv;
            this.XSValue = Xs;

            this.ActualBrush = new SolidColorBrush(Color.FromRgb(231, 23, 23));
        }


        public List<Line> createLines(ColectedData dado, float zoomScale, int index)
        {
            List<Line> objects = new List<Line>

            {
                this.createVa(dado.getVa(index) * zoomScale), //Adiciona Va
                this.createIa(dado.getIa(index) * zoomScale, dado.getFP(index), dado.getFPType(index)), //Adiciona Ia
                this.createXs(dado.getIa(index) * zoomScale, dado.getFP(index), dado.getFPType(index)), //Adiciona Xs
            };

            if (dado.getFP(index) != 0)
            {
                objects.Add(this.createEa());
            }

            return objects;
        }
        public Line createVa(float value)
        {

            // Criar um pincel (brush) usando a cor personalizada
            SolidColorBrush customBrush = new SolidColorBrush(Color.FromRgb(73, 172, 71));

            Line Va = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = (centerX + value),
                Y2 = centerY,
                Stroke = customBrush,
                StrokeThickness = ListData1.configData.getLarguraLinha()
            };
            FinalX = (float)Va.X2;
            FinalY = (float)Va.Y2;
            ActualBrush = customBrush;
            return Va;
        }

        public Line createIa(float value, float FP, char type)
        {
            value = value * ListData1.configData.getIaMultiplier();
            double valueIC;
            double angle = (float)Math.Acos(FP);

            SolidColorBrush customBrush = new SolidColorBrush(Color.FromRgb(231, 23, 23));

            if (type == 'i')
            {
                valueIC = value * Math.Sin(angle); //Ou Ia*sub
            }
            else
            {
                valueIC = (-1) * value * Math.Sin(angle);
            }
            Line Ia = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = centerX + value * Math.Cos(angle),
                Y2 = centerY + valueIC,
                Stroke = customBrush,
                StrokeThickness = ListData1.configData.getLarguraLinha()
            };
            ActualBrush = customBrush;
            return Ia;
        }

        public Line createXs(float IaValue, float FP, char type)
        {
            SolidColorBrush customBrush = new SolidColorBrush(Color.FromRgb(193, 81, 0));
            float angulo = (float)(Math.Acos(FP));
            double altura;
            int typeValue = 1;

            if (type == 'i')
            {
                typeValue = 1; // é indutivo, o Xs é deslocado para a direita
            }
            else
            {
                typeValue = -1; // é capacitivo, o xs é deslocado para a esquerda
            }

            if (this.autoHeigth)
            {
                altura = centerY * 0.95;
            } else
            {
                altura = FinalY - (XSValue * IaValue) * Math.Sin(1.5708 - angulo);
            }

            Line Xs = new Line
            {
                X1 = FinalX,
                Y1 = FinalY,
                X2 = FinalX + typeValue * XSValue * IaValue * Math.Cos(1.5708 - angulo), //Subtrai 90 do angulo.
                Y2 = altura,
                Stroke = customBrush,
                StrokeThickness = ListData1.configData.getLarguraLinha()
            };
            this.FinalX = (float)Xs.X2;
            this.FinalY = (float)Xs.Y2;

            return Xs;
        }

        public Line createEa()
        {
            SolidColorBrush customBrush = new SolidColorBrush(Color.FromRgb(176, 0, 116));

            Line Ea = new Line
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = FinalX,
                Y2 = FinalY,
                Stroke = customBrush,
                StrokeThickness = ListData1.configData.getLarguraLinha()
            };

            return Ea;
        }

        public Path createAngle()
        {
            double angle = 0;
            double radius = 50;
            // Converter ângulo de graus para radianos
            double angleInRadians = angle * Math.PI / 180;

            // Calcular os pontos inicial e final do arco
            double startX = centerX + radius * Math.Cos(angleInRadians / 2);
            double startY = centerY - radius * Math.Sin(angleInRadians / 2);

            double endX = centerX - radius * Math.Cos(angleInRadians / 2);
            double endY = startY;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(startX, startY),
                IsClosed = false
            };

            ArcSegment arcSegment = new ArcSegment
            {
                Size = new Size(radius, radius),
                IsLargeArc = angle > 180,
                SweepDirection = SweepDirection.Clockwise,
                Point = new Point(endX, endY)
            };

            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);

            SolidColorBrush customBrush = new SolidColorBrush(Color.FromArgb(55, 101, 17, 47));

            Path path = new Path
            {
                Stroke = customBrush,
                StrokeThickness = ListData1.configData.getLarguraLinha(),
                Data = pathGeometry
            };

            return path;

        }

        public void setXs(float XsValue)
        {
            this.XSValue = XsValue;
        }

        public void setHeigthOption(bool isActive)
        {
            this.autoHeigth = isActive;
        }
    }
}
