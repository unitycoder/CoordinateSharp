﻿/*
CoordinateSharp is a .NET standard library that is intended to ease geographic coordinate 
format conversions and location based celestial calculations.
https://github.com/Tronald/CoordinateSharp

Many celestial formulas in this library are based on Jean Meeus's 
Astronomical Algorithms (2nd Edition). Comments that reference only a chapter
are referring to this work.

License

CoordinateSharp is split licensed and may be licensed under the GNU Affero General Public License version 3 or a commercial use license as stated.

Copyright (C) 2021, Signature Group, LLC
  
This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 
as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): 
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY Signature Group, LLC. Signature Group, LLC DISCLAIMS THE WARRANTY OF 
NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details. You should have received a copy of the GNU 
Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the 
Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

https://www.gnu.org/licenses/agpl-3.0.html

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, 
as required under Section 5 of the GNU Affero General Public License.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory 
as soon as you develop commercial activities involving the CoordinateSharp software without disclosing the source code of your own applications. 
These activities include: offering paid services to customers as an ASP, on the fly location based calculations in a web application, 
or shipping CoordinateSharp with a closed source product.

Organizations or use cases that fall under the following conditions may receive a free commercial use license upon request on a case by case basis.
	-United States Department of Defense.
	-United States Department of Homeland Security.
	-Open source contributors to this library.
	-Scholarly or scientific research.
	-Emergency response / management uses.

Please visit http://coordinatesharp.com/licensing or contact Signature Group, LLC to purchase a commercial license, or for any questions regarding the AGPL 3.0 license requirements or free use license: sales@signatgroup.com.
*/
using System;
using System.Diagnostics;
namespace CoordinateSharp
{
    [Serializable]
    internal class Distance_Assistant
    {
        /// <summary>
        /// Returns new geodetic coordinate in radians
        /// </summary>
        /// <param name="glat1">Latitude in Radians</param>
        /// <param name="glon1">Longitude in Radians</param>
        /// <param name="faz">Bearing</param>
        /// <param name="s">Distance</param>
        /// <param name="ellipse">Earth Ellipse Values</param>
        /// <returns>double[]</returns>
        public static double[] Direct_Ell(double glat1, double glon1, double faz, double s, double[] ellipse)
        {
            glon1 *= -1; //REVERSE LONG FOR CALC 2.1.1.1
            double EPS = 0.00000000005;//Used to determine if starting at pole.
            double r, tu, sf, cf, b, cu, su, sa, c2a, x, c, d, y, sy = 0, cy = 0, cz = 0, e = 0;
            double glat2, glon2, f;

            //Determine if near pole
            if ((Math.Abs(Math.Cos(glat1)) < EPS) && !(Math.Abs(Math.Sin(faz)) < EPS))
            {
                Debug.WriteLine("Warning: Location is at earth's pole. Only N-S courses are meaningful at this location.");
            }


            double a = ellipse[0];//Equitorial Radius
            f = 1 / ellipse[1];//Flattening
            r = 1 - f;
            tu = r * Math.Tan(glat1);
            sf = Math.Sin(faz);
            cf = Math.Cos(faz);
            if (cf == 0)
            {
                b = 0.0;
            }
            else
            {
                b = 2.0 * Math.Atan2(tu, cf);
            }
            cu = 1.0 / Math.Sqrt(1 + tu * tu);
            su = tu * cu;
            sa = cu * sf;
            c2a = 1 - sa * sa;
            x = 1.0 + Math.Sqrt(1.0 + c2a * (1.0 / (r * r) - 1.0));
            x = (x - 2.0) / x;
            c = 1.0 - x;
            c = (x * x / 4.0 + 1.0) / c;
            d = (0.375 * x * x - 1.0) * x;
            tu = s / (r * a * c);
            y = tu;
            c = y + 1;
            while (Math.Abs(y - c) > EPS)
            {
                sy = Math.Sin(y);
                cy = Math.Cos(y);
                cz = Math.Cos(b + y);
                e = 2.0 * cz * cz - 1.0;
                c = y;
                x = e * cy;
                y = e + e - 1.0;
                y = (((sy * sy * 4.0 - 3.0) * y * cz * d / 6.0 + x) *
                        d / 4.0 - cz) * sy * d + tu;
            }

            b = cu * cy * cf - su * sy;
            c = r * Math.Sqrt(sa * sa + b * b);
            d = su * cy + cu * sy * cf;

            glat2 = ModM.ModLat(Math.Atan2(d, c));
            c = cu * cy - su * sy * cf;
            x = Math.Atan2(sy * sf, c);
            c = ((-3.0 * c2a + 4.0) * f + 4.0) * c2a * f / 16.0;
            d = ((e * cy * c + cz) * sy * c + y) * sa;
            glon2 = ModM.ModLon(glon1 + x - (1.0 - c) * d * f);  //Adjust for IDL
            //baz = ModM.ModCrs(Math.Atan2(sa, b) + Math.PI);
            return new double[] { glat2, glon2 };
        }
        /// <summary>
        /// Returns new geodetic coordinate in radians
        /// </summary>
        /// <param name="lat1">Latitude in radians</param>
        /// <param name="lon1">Longitude in radians</param>
        /// <param name="crs12">Bearing</param>
        /// <param name="d12">Distance</param>
        /// <returns>double[]</returns>
        public static double[] Direct(double lat1, double lon1, double crs12, double d12)
        {
            lon1 *= -1; //REVERSE LONG FOR CALC 2.1.1.1
            var EPS = 0.00000000005;//Used to determine if near pole.
            double dlon, lat, lon;
            d12 = d12 * 0.0005399565; //convert meter to nm
            d12 = d12 / (180 * 60 / Math.PI);//Convert to Radian
            //Determine if near pole
            if ((Math.Abs(Math.Cos(lat1)) < EPS) && !(Math.Abs(Math.Sin(crs12)) < EPS))
            {
                Debug.WriteLine("Warning: Location is at earth's pole. Only N-S courses are meaningful at this location.");
            }

            lat = Math.Asin(Math.Sin(lat1) * Math.Cos(d12) +
                          Math.Cos(lat1) * Math.Sin(d12) * Math.Cos(crs12));
            if (Math.Abs(Math.Cos(lat)) < EPS)
            {
                lon = 0.0; //endpoint a pole
            }
            else
            {
                dlon = Math.Atan2(Math.Sin(crs12) * Math.Sin(d12) * Math.Cos(lat1),
                              Math.Cos(d12) - Math.Sin(lat1) * Math.Sin(lat));
                lon = ModM.Mod(lon1 - dlon + Math.PI, 2 * Math.PI) - Math.PI;
            }

            return new double[] { lat, lon };
        }
        public static double[] Dist_Ell(double glat1, double glon1, double glat2, double glon2, double[] ellipse)
        {
            double a = ellipse[0]; //Equitorial Radius
            double f = 1 / ellipse[1]; //Flattening

            double r, tu1, tu2, cu1, su1, cu2, s1, b1, f1;
            double x = 0, sx = 0, cx = 0, sy = 0, cy = 0, y = 0, sa = 0, c2a = 0, cz = 0, e = 0, c = 0, d = 0;
            double EPS = 0.00000000005;
            double faz, baz, s;
            double iter = 1;
            double MAXITER = 100;
            if ((glat1 + glat2 == 0.0) && (Math.Abs(glon1 - glon2) == Math.PI))
            {
                Debug.WriteLine("Warning: Course and distance between antipodal points is undefined");
                glat1 = glat1 + 0.00001; // allow algorithm to complete
            }
            if (glat1 == glat2 && (glon1 == glon2 || Math.Abs(Math.Abs(glon1 - glon2) - 2 * Math.PI) < EPS))
            {
                Debug.WriteLine("Warning: Points 1 and 2 are identical- course undefined");
                //D
                //crs12
                //crs21
                return new double[] { 0, 0, Math.PI };
            }
            r = 1 - f;
            tu1 = r * Math.Tan(glat1);
            tu2 = r * Math.Tan(glat2);
            cu1 = 1.0 / Math.Sqrt(1.0 + tu1 * tu1);
            su1 = cu1 * tu1;
            cu2 = 1.0 / Math.Sqrt(1.0 + tu2 * tu2);
            s1 = cu1 * cu2;
            b1 = s1 * tu2;
            f1 = b1 * tu1;
            x = glon2 - glon1;
            d = x + 1; // force one pass
            while ((Math.Abs(d - x) > EPS) && (iter < MAXITER))
            {
                iter = iter + 1;
                sx = Math.Sin(x);
                cx = Math.Cos(x);
                tu1 = cu2 * sx;
                tu2 = b1 - su1 * cu2 * cx;
                sy = Math.Sqrt(tu1 * tu1 + tu2 * tu2);
                cy = s1 * cx + f1;
                y = Math.Atan2(sy, cy);
                sa = s1 * sx / sy;
                c2a = 1 - sa * sa;
                cz = f1 + f1;
                if (c2a > 0.0)
                {
                    cz = cy - cz / c2a;
                }
                e = cz * cz * 2.0 - 1.0;
                c = ((-3.0 * c2a + 4.0) * f + 4.0) * c2a * f / 16.0;
                d = x;
                x = ((e * cy * c + cz) * sy * c + y) * sa;
                x = (1.0 - c) * x * f + glon2 - glon1;
            }
            faz = ModM.ModCrs(Math.Atan2(tu1, tu2));
            baz = ModM.ModCrs(Math.Atan2(cu1 * sx, b1 * cx - su1 * cu2) + Math.PI);
            x = Math.Sqrt((1 / (r * r) - 1) * c2a + 1);
            x += 1;
            x = (x - 2.0) / x;
            c = 1.0 - x;
            c = (x * x / 4.0 + 1.0) / c;
            d = (0.375 * x * x - 1.0) * x;
            x = e * cy;
            s = ((((sy * sy * 4.0 - 3.0) * (1.0 - e - e) * cz * d / 6.0 - x) * d / 4.0 + cz) * sy * d + y) * c * a * r;

            if (Math.Abs(iter - MAXITER) < EPS)
            {
                Debug.WriteLine("Warning: Distance algorithm did not converge");
            }

            return new double[] { s, faz, baz };
        }
    }

    /// <summary>
    /// Used for easy read math functions
    /// </summary>
    [Serializable]
    internal static class ModM
    {
        public static double Mod(double x, double y)
        {
            return x - y * Math.Floor(x / y);
        }

        public static double ModLon(double x)
        {
            return Mod(x + Math.PI, 2 * Math.PI) - Math.PI;
        }

        public static double ModCrs(double x)
        {
            return Mod(x, 2 * Math.PI);
        }

        public static double ModLat(double x)
        {
            return Mod(x + Math.PI / 2, 2 * Math.PI) - Math.PI / 2;
        }
    }
}
