using System;
using System.Diagnostics;

namespace SunriseCalculator
{
    public class SolarInfo
    {
        public double SolarDeclination { get; private set; }
        public TimeSpan EquationOfTime { get; private set; }
        public DateTime Sunrise { get; private set; }
        public DateTime Sunset { get; private set; }
        public TimeSpan? Noon { get; private set; }
        public DateTime Date { get; private set; }

        private SolarInfo() { }

        public static SolarInfo ForDate(double latitude, double longitude, DateTime date)
        {
            var info = new SolarInfo();
            info.Date = date = date.Date;

            var year = date.Year;
            var month = date.Month;
            var day = date.Day;

            if ((latitude >= -90) && (latitude < -89))
            {
                //alert("All latitudes between 89 and 90 S\n will be set to -89");
                //latLongForm["latDeg"].value = -89;
                latitude = -89;
            }

            if ((latitude <= 90) && (latitude > 89))
            {
                //alert("All latitudes between 89 and 90 N\n will be set to 89");
                //latLongForm["latDeg"].value = 89;
                latitude = 89;
            }

            //*****	Calculate the time of sunrise			
            var JD = calcJD(year, month, day);
            //var dow = calcDayOfWeek(JD);
            var doy = calcDayOfYear(month, day, isLeapYear(year));
            var T = calcTimeJulianCent(JD);

            // var alpha = calcSunRtAscension(T);
            var solarDec = calcSunDeclination(T);
            var eqTime = calcEquationOfTime(T); // (in minutes)



            // Calculate sunrise for this date
            // if no sunrise is found, set flag nosunrise
            var nosunrise = false;

            var riseTimeGMT = calcSunriseUTC(JD, latitude, longitude);
            nosunrise = !isNumber(riseTimeGMT);

            // Calculate sunset for this date
            // if no sunset is found, set flag nosunset
            var nosunset = false;
            var setTimeGMT = calcSunsetUTC(JD, latitude, longitude);
            if (!isNumber(setTimeGMT))
            {
                nosunset = true;
            }

            if (!nosunrise)		// Sunrise was found
            {
                info.Sunrise = date.Date.AddMinutes(riseTimeGMT);
            }

            if (!nosunset)		// Sunset was found
            {
                info.Sunset = date.Date.AddMinutes(setTimeGMT);
            }

            // Calculate solar noon for this date
            var solNoonGMT = calcSolNoonUTC(T, longitude);

            if (!(nosunset || nosunrise))
            {
                info.Noon = TimeSpan.FromMinutes(solNoonGMT);
            }


            var tsnoon = calcTimeJulianCent(calcJDFromJulianCent(T) - 0.5 + solNoonGMT / 1440.0);
            eqTime = calcEquationOfTime(tsnoon);
            solarDec = calcSunDeclination(tsnoon);

            info.EquationOfTime = TimeSpan.FromMinutes(eqTime);
            info.SolarDeclination = solarDec;

            // report special cases of no sunrise
            if (nosunrise)
            {
                // if Northern hemisphere and spring or summer, OR  
                // if Southern hemisphere and fall or winter, use 
                // previous sunrise and next sunset

                if (((latitude > 66.4) && (doy > 79) && (doy < 267)) ||
                   ((latitude < -66.4) && ((doy < 83) || (doy > 263))))
                {
                    var newjd = findRecentSunrise(JD, latitude, longitude);
                    var newtime = calcSunriseUTC(newjd, latitude, longitude);

                    if (newtime > 1440)
                    {
                        newtime -= 1440;
                        newjd += 1.0;
                    }
                    if (newtime < 0)
                    {
                        newtime += 1440;
                        newjd -= 1.0;
                    }

                    info.Sunrise = ConvertToDate(newtime, newjd);
                }

                // if Northern hemisphere and fall or winter, OR 
                // if Southern hemisphere and spring or summer, use 
                // next sunrise and previous sunset

                else if (((latitude > 66.4) && ((doy < 83) || (doy > 263))) ||
                    ((latitude < -66.4) && (doy > 79) && (doy < 267)))
                {
                    var newjd = findNextSunrise(JD, latitude, longitude);
                    var newtime = calcSunriseUTC(newjd, latitude, longitude);

                    if (newtime > 1440)
                    {
                        newtime -= 1440;
                        newjd += 1.0;
                    }
                    if (newtime < 0)
                    {
                        newtime += 1440;
                        newjd -= 1.0;
                    }

                    info.Sunrise = ConvertToDate(newtime, newjd);
                }
                else
                {
                    Debug.Fail("Cannot Find Sunrise!");
                }

                // alert("Last Sunrise was on day " + findRecentSunrise(JD, latitude, longitude));
                // alert("Next Sunrise will be on day " + findNextSunrise(JD, latitude, longitude));
            }

            if (nosunset)
            {
                // if Northern hemisphere and spring or summer, OR
                // if Southern hemisphere and fall or winter, use 
                // previous sunrise and next sunset

                if (((latitude > 66.4) && (doy > 79) && (doy < 267)) ||
                   ((latitude < -66.4) && ((doy < 83) || (doy > 263))))
                {
                    var newjd = findNextSunset(JD, latitude, longitude);
                    var newtime = calcSunsetUTC(newjd, latitude, longitude);

                    if (newtime > 1440)
                    {
                        newtime -= 1440;
                        newjd += 1.0;
                    }
                    if (newtime < 0)
                    {
                        newtime += 1440;
                        newjd -= 1.0;
                    }

                    info.Sunset = ConvertToDate(newtime, newjd);
                }

                // if Northern hemisphere and fall or winter, OR
                // if Southern hemisphere and spring or summer, use 
                // next sunrise and last sunset

                else if (((latitude > 66.4) && ((doy < 83) || (doy > 263))) ||
                    ((latitude < -66.4) && (doy > 79) && (doy < 267)))
                {
                    var newjd = findRecentSunset(JD, latitude, longitude);
                    var newtime = calcSunsetUTC(newjd, latitude, longitude);

                    if (newtime > 1440)
                    {
                        newtime -= 1440;
                        newjd += 1.0;
                    }
                    if (newtime < 0)
                    {
                        newtime += 1440;
                        newjd -= 1.0;
                    }

                    info.Sunset = ConvertToDate(newtime, newjd);
                }
                else
                {
                    Debug.Fail("Cannot Find Sunset!");
                }

            }

            return info;
        }

        // This is inspired by timeStringShortAMPM from the original source.
        /// <summary>Note: This treats fractional julian days as whole days, so the minutes portion of the julian day will be replaced with the value of the minutes parameter.</summary>
        private static DateTime ConvertToDate(double minutes, double JD)
        {
            var julianday = JD;
            var floatHour = minutes / 60.0;
            var hour = Math.Floor(floatHour);
            var floatMinute = 60.0 * (floatHour - Math.Floor(floatHour));
            var minute = Math.Floor(floatMinute);
            var floatSec = 60.0 * (floatMinute - Math.Floor(floatMinute));
            var second = Math.Floor(floatSec + 0.5);

            minute += (second >= 30) ? 1 : 0;

            if (minute >= 60)
            {
                minute -= 60;
                hour++;
            }

            if (hour > 23)
            {
                hour -= 24;
                julianday += 1.0;
            }

            if (hour < 0)
            {
                hour += 24;
                julianday -= 1.0;
            }

            return calcDayFromJD(julianday).Add(new TimeSpan(0, (int)hour, (int)minute, (int)second));
        }

        private static DateTime calcDayFromJD(double jd)
        {
            var z = Math.Floor(jd + 0.5);
            var f = (jd + 0.5) - z;

            double A = 0;
            if (z < 2299161)
            {
                A = z;
            }
            else
            {
                var alpha = Math.Floor((z - 1867216.25) / 36524.25);
                A = z + 1 + alpha - Math.Floor(alpha / 4);
            }

            var B = A + 1524;
            var C = Math.Floor((B - 122.1) / 365.25);
            var D = Math.Floor(365.25 * C);
            var E = Math.Floor((B - D) / 30.6001);

            var day = B - D - Math.Floor(30.6001 * E) + f;
            var month = (E < 14) ? E - 1 : E - 13;
            var year = (month > 2) ? C - 4716 : C - 4715;

            return new DateTime((int)year, (int)month, (int)day, 0, 0, 0, DateTimeKind.Utc);
        }

        private static bool isLeapYear(int yr)
        {
            return ((yr % 4 == 0 && yr % 100 != 0) || yr % 400 == 0);
        }


        //*********************************************************************/

        // isPosInteger returns false if the value is not a positive integer, true is
        // returned otherwise.  The code is from taken from Danny Goodman's Javascript
        // Handbook, p. 372.

        private static bool isPosInteger(int inputVal)
        {
            return inputVal > 0;
        }

        private static bool isNumber(double inputVal)
        {
            //var oneDecimal = false;
            //var inputStr = "" + inputVal;
            //for (var i = 0; i < inputStr.length; i++) 
            //{
            //    var oneChar = inputStr.charAt(i);
            //    if (i == 0 && (oneChar == "-" || oneChar == "+"))
            //    {
            //        continue;
            //    }
            //    if (oneChar == "." && !oneDecimal) 
            //    {
            //        oneDecimal = true;
            //        continue;
            //    }
            //    if (oneChar < "0" || oneChar > "9")
            //    {
            //        return false;
            //    }
            //}
            //return true;

            // TODO: Make sure I ported this function correctly
            return !double.IsNaN(inputVal);
        }

        // Convert radian angle to degrees
        private static double radToDeg(double angleRad)
        {
            return (180.0 * angleRad / Math.PI);
        }


        // Convert degree angle to radians
        private static double degToRad(double angleDeg)
        {
            return (Math.PI * angleDeg / 180.0);
        }

        //***********************************************************************/
        //* Name:    calcDayOfYear								*/
        //* Type:    Function									*/
        //* Purpose: Finds numerical day-of-year from mn, day and lp year info  */
        //* Arguments:										*/
        //*   month: January = 1								*/
        //*   day  : 1 - 31									*/
        //*   lpyr : 1 if leap year, 0 if not						*/
        //* Return value:										*/
        //*   The numerical day of year							*/
        //***********************************************************************/
        private static int calcDayOfYear(int mn, int dy, bool lpyr)
        {
            var k = (lpyr ? 1 : 2);
            var doy = Math.Floor((275d * mn) / 9d) - k * Math.Floor((mn + 9d) / 12d) + dy - 30;
            return (int)doy;
        }


        //***********************************************************************/
        //* Name:    calcDayOfWeek								*/
        //* Type:    Function									*/
        //* Purpose: Derives weekday from Julian Day					*/
        //* Arguments:										*/
        //*   juld : Julian Day									*/
        //* Return value:										*/
        //*   String containing name of weekday						*/
        //***********************************************************************/
        private static string calcDayOfWeek(double juld)
        {
            var A = (juld + 1.5) % 7;
            var DOW = (A == 0) ? "Sunday" : (A == 1) ? "Monday" : (A == 2) ? "Tuesday" : (A == 3) ? "Wednesday" : (A == 4) ? "Thursday" : (A == 5) ? "Friday" : "Saturday";
            return DOW;
        }

        //***********************************************************************/
        //* Name:    calcJD									*/
        //* Type:    Function									*/
        //* Purpose: Julian day from calendar day						*/
        //* Arguments:										*/
        //*   year : 4 digit year								*/
        //*   month: January = 1								*/
        //*   day  : 1 - 31									*/
        //* Return value:										*/
        //*   The Julian day corresponding to the date					*/
        //* Note:											*/
        //*   Number is returned for start of day.  Fractional days should be	*/
        //*   added later.									*/
        //***********************************************************************/
        private static double calcJD(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            var A = Math.Floor(year / 100d);
            var B = 2 - A + Math.Floor(A / 4d);

            var JD = Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1.0)) + day + B - 1524.5;

            return JD;
        }

        //***********************************************************************/
        //* Name:    calcTimeJulianCent							*/
        //* Type:    Function									*/
        //* Purpose: convert Julian Day to centuries since J2000.0.			*/
        //* Arguments:										*/
        //*   jd : the Julian Day to convert						*/
        //* Return value:										*/
        //*   the T value corresponding to the Julian Day				*/
        //***********************************************************************/
        private static double calcTimeJulianCent(double jd)
        {
            var T = (jd - 2451545.0) / 36525.0;
            return T;
        }


        //***********************************************************************/
        //* Name:    calcJDFromJulianCent							*/
        //* Type:    Function									*/
        //* Purpose: convert centuries since J2000.0 to Julian Day.			*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   the Julian Day corresponding to the t value				*/
        //***********************************************************************/
        private static double calcJDFromJulianCent(double t)
        {
            var JD = t * 36525.0 + 2451545.0;
            return JD;
        }

        //***********************************************************************/
        //* Name:    calGeomMeanLongSun							*/
        //* Type:    Function									*/
        //* Purpose: calculate the Geometric Mean Longitude of the Sun		*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   the Geometric Mean Longitude of the Sun in degrees			*/
        //***********************************************************************/
        private static double calcGeomMeanLongSun(double t)
        {
            var L0 = 280.46646 + t * (36000.76983 + 0.0003032 * t);
            while (L0 > 360.0)
            {
                L0 -= 360.0;
            }
            while (L0 < 0.0)
            {
                L0 += 360.0;
            }
            return L0;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calGeomAnomalySun							*/
        //* Type:    Function									*/
        //* Purpose: calculate the Geometric Mean Anomaly of the Sun		*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   the Geometric Mean Anomaly of the Sun in degrees			*/
        //***********************************************************************/
        private static double calcGeomMeanAnomalySun(double t)
        {
            var M = 357.52911 + t * (35999.05029 - 0.0001537 * t);
            return M;		// in degrees
        }


        //***********************************************************************/
        //* Name:    calcEccentricityEarthOrbit						*/
        //* Type:    Function									*/
        //* Purpose: calculate the eccentricity of earth's orbit			*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   the unitless eccentricity							*/
        //***********************************************************************/
        private static double calcEccentricityEarthOrbit(double t)
        {
            var e = 0.016708634 - t * (0.000042037 + 0.0000001267 * t);
            return e;		// unitless
        }

        //***********************************************************************/
        //* Name:    calcSunEqOfCenter							*/
        //* Type:    Function									*/
        //* Purpose: calculate the equation of center for the sun			*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   in degrees										*/
        //***********************************************************************/
        private static double calcSunEqOfCenter(double t)
        {
            var m = calcGeomMeanAnomalySun(t);

            var mrad = degToRad(m);
            var sinm = Math.Sin(mrad);
            var sin2m = Math.Sin(mrad + mrad);
            var sin3m = Math.Sin(mrad + mrad + mrad);

            var C = sinm * (1.914602 - t * (0.004817 + 0.000014 * t)) + sin2m * (0.019993 - 0.000101 * t) + sin3m * 0.000289;
            return C;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calcSunTrueLong								*/
        //* Type:    Function									*/
        //* Purpose: calculate the true longitude of the sun				*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun's true longitude in degrees						*/
        //***********************************************************************/
        private static double calcSunTrueLong(double t)
        {
            var l0 = calcGeomMeanLongSun(t);
            var c = calcSunEqOfCenter(t);

            var O = l0 + c;
            return O;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calcSunTrueAnomaly							*/
        //* Type:    Function									*/
        //* Purpose: calculate the true anamoly of the sun				*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun's true anamoly in degrees							*/
        //***********************************************************************/
        private static double calcSunTrueAnomaly(double t)
        {
            var m = calcGeomMeanAnomalySun(t);
            var c = calcSunEqOfCenter(t);

            var v = m + c;
            return v;		// in degrees
        }



        //***********************************************************************/
        //* Name:    calcSunRadVector								*/
        //* Type:    Function									*/
        //* Purpose: calculate the distance to the sun in AU				*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun radius vector in AUs							*/
        //***********************************************************************/
        private static double calcSunRadVector(double t)
        {
            var v = calcSunTrueAnomaly(t);
            var e = calcEccentricityEarthOrbit(t);

            var R = (1.000001018 * (1 - e * e)) / (1 + e * Math.Cos(degToRad(v)));
            return R;		// in AUs
        }



        //***********************************************************************/
        //* Name:    calcSunApparentLong							*/
        //* Type:    Function									*/
        //* Purpose: calculate the apparent longitude of the sun			*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun's apparent longitude in degrees						*/
        //***********************************************************************/
        private static double calcSunApparentLong(double t)
        {
            var o = calcSunTrueLong(t);

            var omega = 125.04 - 1934.136 * t;
            var lambda = o - 0.00569 - 0.00478 * Math.Sin(degToRad(omega));
            return lambda;		// in degrees
        }



        //***********************************************************************/
        //* Name:    calcMeanObliquityOfEcliptic						*/
        //* Type:    Function									*/
        //* Purpose: calculate the mean obliquity of the ecliptic			*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   mean obliquity in degrees							*/
        //***********************************************************************/
        private static double calcMeanObliquityOfEcliptic(double t)
        {
            var seconds = 21.448 - t * (46.8150 + t * (0.00059 - t * (0.001813)));
            var e0 = 23.0 + (26.0 + (seconds / 60.0)) / 60.0;
            return e0;		// in degrees
        }



        //***********************************************************************/
        //* Name:    calcObliquityCorrection						*/
        //* Type:    Function									*/
        //* Purpose: calculate the corrected obliquity of the ecliptic		*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   corrected obliquity in degrees						*/
        //***********************************************************************/
        private static double calcObliquityCorrection(double t)
        {
            var e0 = calcMeanObliquityOfEcliptic(t);

            var omega = 125.04 - 1934.136 * t;
            var e = e0 + 0.00256 * Math.Cos(degToRad(omega));
            return e;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calcSunRtAscension							*/
        //* Type:    Function									*/
        //* Purpose: calculate the right ascension of the sun				*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun's right ascension in degrees						*/
        //***********************************************************************/
        private static double calcSunRtAscension(double t)
        {
            var e = calcObliquityCorrection(t);
            var lambda = calcSunApparentLong(t);

            var tananum = (Math.Cos(degToRad(e)) * Math.Sin(degToRad(lambda)));
            var tanadenom = (Math.Cos(degToRad(lambda)));
            var alpha = radToDeg(Math.Atan2(tananum, tanadenom));
            return alpha;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calcSunDeclination							*/
        //* Type:    Function									*/
        //* Purpose: calculate the declination of the sun				*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   sun's declination in degrees							*/
        //***********************************************************************/
        private static double calcSunDeclination(double t)
        {
            var e = calcObliquityCorrection(t);
            var lambda = calcSunApparentLong(t);

            var sint = Math.Sin(degToRad(e)) * Math.Sin(degToRad(lambda));
            var theta = radToDeg(Math.Asin(sint));
            return theta;		// in degrees
        }

        //***********************************************************************/
        //* Name:    calcEquationOfTime							*/
        //* Type:    Function									*/
        //* Purpose: calculate the difference between true solar time and mean	*/
        //*		solar time									*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //* Return value:										*/
        //*   equation of time in minutes of time						*/
        //***********************************************************************/
        private static double calcEquationOfTime(double t)
        {
            var epsilon = calcObliquityCorrection(t);
            var l0 = calcGeomMeanLongSun(t);
            var e = calcEccentricityEarthOrbit(t);
            var m = calcGeomMeanAnomalySun(t);

            var y = Math.Tan(degToRad(epsilon) / 2.0);
            y *= y;

            var sin2l0 = Math.Sin(2.0 * degToRad(l0));
            var sinm = Math.Sin(degToRad(m));
            var cos2l0 = Math.Cos(2.0 * degToRad(l0));
            var sin4l0 = Math.Sin(4.0 * degToRad(l0));
            var sin2m = Math.Sin(2.0 * degToRad(m));

            var Etime = y * sin2l0 - 2.0 * e * sinm + 4.0 * e * y * sinm * cos2l0
                    - 0.5 * y * y * sin4l0 - 1.25 * e * e * sin2m;

            return radToDeg(Etime) * 4.0;	// in minutes of time
        }

        //***********************************************************************/
        //* Name:    calcHourAngleSunrise							*/
        //* Type:    Function									*/
        //* Purpose: calculate the hour angle of the sun at sunrise for the	*/
        //*			latitude								*/
        //* Arguments:										*/
        //*   lat : latitude of observer in degrees					*/
        //*	solarDec : declination angle of sun in degrees				*/
        //* Return value:										*/
        //*   hour angle of sunrise in radians						*/
        //***********************************************************************/
        private static double calcHourAngleSunrise(double lat, double solarDec)
        {
            var latRad = degToRad(lat);
            var sdRad = degToRad(solarDec);

            var HAarg = (Math.Cos(degToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad));
            var HA = (Math.Acos(Math.Cos(degToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad)));
            return HA;		// in radians
        }

        //***********************************************************************/
        //* Name:    calcHourAngleSunset							*/
        //* Type:    Function									*/
        //* Purpose: calculate the hour angle of the sun at sunset for the	*/
        //*			latitude								*/
        //* Arguments:										*/
        //*   lat : latitude of observer in degrees					*/
        //*	solarDec : declination angle of sun in degrees				*/
        //* Return value:										*/
        //*   hour angle of sunset in radians						*/

        private static double calcHourAngleSunset(double lat, double solarDec)
        {
            var latRad = degToRad(lat);
            var sdRad = degToRad(solarDec);

            var HAarg = (Math.Cos(degToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad));

            var HA = (Math.Acos(Math.Cos(degToRad(90.833)) / (Math.Cos(latRad) * Math.Cos(sdRad)) - Math.Tan(latRad) * Math.Tan(sdRad)));

            return -HA;		// in radians
        }


        //***********************************************************************/
        //* Name:    calcSunriseUTC								*/
        //* Type:    Function									*/
        //* Purpose: calculate the Universal Coordinated Time (UTC) of sunrise	*/
        //*			for the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   time in minutes from zero Z							*/
        //***********************************************************************/
        private static double calcSunriseUTC(double JD, double latitude, double longitude)
        {
            var t = calcTimeJulianCent(JD);

            // *** Find the time of solar noon at the location, and use
            //     that declination. This is better than start of the 
            //     Julian day

            var noonmin = calcSolNoonUTC(t, longitude);
            var tnoon = calcTimeJulianCent(JD + noonmin / 1440.0);

            // *** First pass to approximate sunrise (using solar noon)

            var eqTime = calcEquationOfTime(tnoon);
            var solarDec = calcSunDeclination(tnoon);
            var hourAngle = calcHourAngleSunrise(latitude, solarDec);

            var delta = longitude - radToDeg(hourAngle);
            var timeDiff = 4 * delta;	// in minutes of time
            var timeUTC = 720 + timeDiff - eqTime;	// in minutes

            // alert("eqTime = " + eqTime + "\nsolarDec = " + solarDec + "\ntimeUTC = " + timeUTC);

            // *** Second pass includes fractional jday in gamma calc

            var newt = calcTimeJulianCent(calcJDFromJulianCent(t) + timeUTC / 1440.0);
            eqTime = calcEquationOfTime(newt);
            solarDec = calcSunDeclination(newt);
            hourAngle = calcHourAngleSunrise(latitude, solarDec);
            delta = longitude - radToDeg(hourAngle);
            timeDiff = 4 * delta;
            timeUTC = 720 + timeDiff - eqTime; // in minutes

            // alert("eqTime = " + eqTime + "\nsolarDec = " + solarDec + "\ntimeUTC = " + timeUTC);

            return timeUTC;
        }

        //***********************************************************************/
        //* Name:    calcSolNoonUTC								*/
        //* Type:    Function									*/
        //* Purpose: calculate the Universal Coordinated Time (UTC) of solar	*/
        //*		noon for the given day at the given location on earth		*/
        //* Arguments:										*/
        //*   t : number of Julian centuries since J2000.0				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   time in minutes from zero Z							*/
        //***********************************************************************/

        private static double calcSolNoonUTC(double t, double longitude)
        {
            // First pass uses approximate solar noon to calculate eqtime
            var tnoon = calcTimeJulianCent(calcJDFromJulianCent(t) + longitude / 360.0);
            var eqTime = calcEquationOfTime(tnoon);
            var solNoonUTC = 720 + (longitude * 4) - eqTime; // min

            var newt = calcTimeJulianCent(calcJDFromJulianCent(t) - 0.5 + solNoonUTC / 1440.0);

            eqTime = calcEquationOfTime(newt);
            // var solarNoonDec = calcSunDeclination(newt);
            solNoonUTC = 720 + (longitude * 4) - eqTime; // min

            return solNoonUTC;
        }

        //***********************************************************************/
        //* Name:    calcSunsetUTC								*/
        //* Type:    Function									*/
        //* Purpose: calculate the Universal Coordinated Time (UTC) of sunset	*/
        //*			for the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   time in minutes from zero Z							*/
        //***********************************************************************/
        private static double calcSunsetUTC(double JD, double latitude, double longitude)
        {
            var t = calcTimeJulianCent(JD);

            // *** Find the time of solar noon at the location, and use
            //     that declination. This is better than start of the 
            //     Julian day

            var noonmin = calcSolNoonUTC(t, longitude);
            var tnoon = calcTimeJulianCent(JD + noonmin / 1440.0);

            // First calculates sunrise and approx length of day

            var eqTime = calcEquationOfTime(tnoon);
            var solarDec = calcSunDeclination(tnoon);
            var hourAngle = calcHourAngleSunset(latitude, solarDec);

            var delta = longitude - radToDeg(hourAngle);
            var timeDiff = 4 * delta;
            var timeUTC = 720 + timeDiff - eqTime;

            // first pass used to include fractional day in gamma calc

            var newt = calcTimeJulianCent(calcJDFromJulianCent(t) + timeUTC / 1440.0);
            eqTime = calcEquationOfTime(newt);
            solarDec = calcSunDeclination(newt);
            hourAngle = calcHourAngleSunset(latitude, solarDec);

            delta = longitude - radToDeg(hourAngle);
            timeDiff = 4 * delta;
            timeUTC = 720 + timeDiff - eqTime; // in minutes

            return timeUTC;
        }

        //***********************************************************************/
        //* Name:    findRecentSunrise							*/
        //* Type:    Function									*/
        //* Purpose: calculate the julian day of the most recent sunrise		*/
        //*		starting from the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   julian day of the most recent sunrise					*/
        //***********************************************************************/
        private static double findRecentSunrise(double jd, double latitude, double longitude)
        {
            var julianday = jd;

            var time = calcSunriseUTC(julianday, latitude, longitude);
            while (!isNumber(time))
            {
                julianday -= 1.0;
                time = calcSunriseUTC(julianday, latitude, longitude);
            }

            return julianday;
        }


        //***********************************************************************/
        //* Name:    findRecentSunset								*/
        //* Type:    Function									*/
        //* Purpose: calculate the julian day of the most recent sunset		*/
        //*		starting from the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   julian day of the most recent sunset					*/
        //***********************************************************************/
        private static double findRecentSunset(double jd, double latitude, double longitude)
        {
            var julianday = jd;

            var time = calcSunsetUTC(julianday, latitude, longitude);
            while (!isNumber(time))
            {
                julianday -= 1.0;
                time = calcSunsetUTC(julianday, latitude, longitude);
            }

            return julianday;
        }


        //***********************************************************************/
        //* Name:    findNextSunrise								*/
        //* Type:    Function									*/
        //* Purpose: calculate the julian day of the next sunrise			*/
        //*		starting from the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   julian day of the next sunrise						*/
        //***********************************************************************/
        private static double findNextSunrise(double jd, double latitude, double longitude)
        {
            var julianday = jd;

            var time = calcSunriseUTC(julianday, latitude, longitude);
            while (!isNumber(time))
            {
                julianday += 1.0;

                time = calcSunriseUTC(julianday, latitude, longitude);

            }

            return julianday;
        }


        //***********************************************************************/
        //* Name:    findNextSunset								*/
        //* Type:    Function									*/
        //* Purpose: calculate the julian day of the next sunset			*/
        //*		starting from the given day at the given location on earth	*/
        //* Arguments:										*/
        //*   JD  : julian day									*/
        //*   latitude : latitude of observer in degrees				*/
        //*   longitude : longitude of observer in degrees				*/
        //* Return value:										*/
        //*   julian day of the next sunset							*/
        //***********************************************************************/
        private static double findNextSunset(double jd, double latitude, double longitude)
        {
            var julianday = jd;

            var time = calcSunsetUTC(julianday, latitude, longitude);
            while (!isNumber(time))
            {
                julianday += 1.0;
                time = calcSunsetUTC(julianday, latitude, longitude);
            }

            return julianday;
        }
    }
}