using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
//using System.DirectoryServices;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Management.Automation;
using Microsoft.VisualBasic.CompilerServices;
using System.Web.Http;

namespace SMM.Helper
{
    public static class Extensions
    {
        /// <summary>
        /// Helper for returning the name of a given controller. Helpful when calling
        /// an authorization routine that loads rules based on controller name.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string ControllerName (this ApiController conn)
        {
            return conn.ControllerContext.RouteData.Values["controller"].ToString();
        }

        public static HttpResponseMessage AsAppJsonResult(this HttpRequestMessage Request, IEnumerable<object> input = null) {

            var res = Request.CreateResponse(HttpStatusCode.OK);
            var resultString = String.Concat(input?.Select(x => x.ToString().ToList()));

            res.Content = new StringContent(resultString, Encoding.UTF8, "application/json");
            return res;
        }

        /// <summary>
        /// VisualBasic's string comparison with wildcard support.
        /// </summary>
        /// <param name="Base">The value to check.</param>
        /// <param name="Pattern">The pattern compared to 'Base'. Supports wildcards
        /// and other niceties. More info: https://docs.microsoft.com/en-us/office/vba/Language/Reference/User-Interface-Help/wildcard-characters-used-in-string-comparisons.
        /// </param>
        /// <returns></returns>
        public static bool Like(this string Base, string Pattern)
        {
            return LikeOperator.LikeString(Base, Pattern, Microsoft.VisualBasic.CompareMethod.Text);
        }

        #region PSObjectHandling
        /// <summary>
        /// Naivly unboxes a single PSObject into a Dictionary(string, object).
        /// If the PSObject is a string, it is stored as "Output": value.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDict(this PSObject input)
        {
            var result = new Dictionary<string, object>();
            if (input.TypeNames.Contains(typeof(string).FullName))
            {
                result.Add("Output", String.Copy(input.ToString()));
            }
            else
            {
                foreach (var p in input.Properties)
                {
                    result.Add(p.Name, p.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Unboxes a collection of PSObjects, returning as a List(Dictionary(string, object)).
        /// The resulting value is not very ergonomic but serializes to JSON well.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDict(this PSDataCollection<PSObject> input)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (var i in input)
            {
                result.Add(i.ToDict());
            }
            return result;
        }

        /// <summary>
        /// Attempts to map properties from the PSObject to properties from the
        /// desired result type using a case insensitive property name match.
        /// If successful, the dynamic result will be of the desired type.
        /// </summary>
        /// <typeparam name="T">The type to map to.</typeparam>
        /// <param name="input">The PSObject we're mapping values from.</param>
        /// <returns></returns>
        public static dynamic CapturePSResult<T>(this PSObject input)
        {
            dynamic result = default(dynamic);
            var asType = typeof(T);

            bool captureSuccess = true;

            try {
                result = Activator.CreateInstance(asType);
                var hintType = asType.GetTypeInfo();
                var props = hintType.DeclaredProperties;
                foreach (var p in props)
                {
                    var psprop = input.Properties.FirstOrDefault(x => x.Name.Like(p.Name));
                    if (psprop != null)
                    {
                        if (psprop.Value is PSObject psobj)
                        {
                            p.SetValue(result, psobj.ToDict());
                        }
                        else if (psprop.Value.GetType() == typeof(System.Management.Automation.PSCustomObject))
                        {
                            PSCustomObject custompsobj = (PSCustomObject) psprop.Value;
                            var val = custompsobj.CapturePSResult<T>();
                            p.SetValue(result, val);
                        }
                        else
                        {
                            p.SetValue(result, psprop.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.GetAllMessages());
                captureSuccess = false;
            }

            // Capturing the PSObject as the desired type failed.
            // Fall back to returning a Dictionary<string, object>.
            if (!captureSuccess) result = input.ToDict();

            return result;
        }


        /// <summary>
        /// Attempts to map properties from the PSObject to properties from the
        /// desired result type using a case insensitive property name match.
        /// If successful, the dynamic result will be of the desired type.
        /// </summary>
        /// <typeparam name="T">The type to map to.</typeparam>
        /// <param name="input">The PSObject we're mapping values from.</param>
        /// <returns></returns>
        public static dynamic CapturePSResult<T>(this PSCustomObject input)
        {
            throw new NotImplementedException("This has not been tested!!!");

            dynamic result = default(dynamic);
            var asType = typeof(T);
            var inType = typeof(PSCustomObject);
            bool captureSuccess = true;

            try
            {
                result = Activator.CreateInstance(asType);
                var hintType = asType.GetTypeInfo();
                var props = hintType.DeclaredProperties;
                foreach (var p in props)
                {
                    var psprop = inType.GetProperties().FirstOrDefault(x => x.Name.Like(p.Name));
                    if (psprop != null)
                    {
                       p.SetValue(result, psprop);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.GetAllMessages());
                captureSuccess = false;
            }

            // Capturing the PSObject as the desired type failed.
            // Fall back to returning a Dictionary<string, object>.
            //if (!captureSuccess) result = input.ToDict();

            return result;
        }

        /// <summary>
        /// Loop through the collection and capture the PSObjects as the desired type T.
        /// If the mapping fails, results are returned as a Dictionary(string, object) so
        /// though the return type is dynamic, it should be checked with the as operator
        /// or GetType().
        /// </summary>
        /// <typeparam name="T">Desired result type.</typeparam>
        /// <param name="input">PSObject collection to capture values from.</param>
        /// <returns></returns>
        public static List<dynamic> CapturePSResult<T>(this PSDataCollection<PSObject> input)
        {
            var result = new List<dynamic>();
            var asType = typeof(T);

            foreach (var i in input)
            {
                result.Add(i.CapturePSResult<T>());
            }

            return result;
        }
        #endregion  // PSObjectHandling

        private readonly static object _lock = new object();

        public static T CloneObject<T>(T original)
        {
            try
            {
                Monitor.Enter(_lock);
                T copy = Activator.CreateInstance<T>();
                PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo pi in piList)
                {
                    if (pi.GetValue(copy, null) != pi.GetValue(original, null))
                    {
                        try
                        {
                            pi.SetValue(copy, pi.GetValue(original, null), null);
                        }
                        catch (Exception e) when (e.Message == "Property set method not found.")
                        {
                            // I don't care about not being able to set private properties
                        }
                    }
                }
                return copy;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static T CloneObject<T>(T original, List<string> propertyExcludeList)
        {
            try
            {
                Monitor.Enter(_lock);
                T copy = Activator.CreateInstance<T>();
                PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo pi in piList)
                {
                    if (!propertyExcludeList.Contains(pi.Name))
                    {
                        if (pi.GetValue(copy, null) != pi.GetValue(original, null))
                        {
                            pi.SetValue(copy, pi.GetValue(original, null), null);
                        }
                    }
                }
                return copy;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static string GetAllMessages(this Exception exception)
        {
            var messages = exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => ex.Message);
            return String.Join(Environment.NewLine, messages);
        }

        //public static void GetValueFromDirectoryEntry<T>(this T obj, DirectoryEntry entry, string name)
        //{

        //    var utype = obj.GetType();
        //    var p = utype.GetProperty(name);

        //    try
        //    {
        //        var val = entry?.Properties[p.Name]?.Value;
        //        if (val == null) return;
        //        Type propType = p.PropertyType;
        //        if (val?.GetType() == typeof(object[]))
        //        {
        //            var temp = Activator.CreateInstance(propType);
        //            var ctor = utype.GetConstructor(new Type[] { propType });
        //            if (propType.GetMethods().Any(x => x.Name == "AddRange"))
        //            {
        //                var val_arr = (val as object[]).Cast<string>();
        //                temp.GetType().GetMethod("AddRange").Invoke(temp, new object[] { val_arr });
        //            }
        //            p.SetValue(obj, temp);
        //        }
        //        else
        //        {
        //            p.SetValue(obj, val);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.WriteLine($"Error setting property: {p.Name}");
        //        Trace.WriteLine(e.Message);
        //        Trace.WriteLine(e.StackTrace);
        //    }
        //}

        // TODO reimplement with win32
        ///// <summary>
        /////     Intent:  
        /////     - Shift the window onto the visible screen.
        /////     - Shift the window away from overlapping the task bar.
        ///// </summary>
        //public static void ShiftWindowOntoScreen(this System.Windows.Window window)
        //{
        //    // Note that "window.BringIntoView()" does not work.                            
        //    if (window.Top < System.Windows.SystemParameters.VirtualScreenTop)
        //    {
        //        window.Top = System.Windows.SystemParameters.VirtualScreenTop;
        //    }

        //    if (window.Left < System.Windows.SystemParameters.VirtualScreenLeft)
        //    {
        //        window.Left = System.Windows.SystemParameters.VirtualScreenLeft;
        //    }

        //    if (window.Left + window.Width > System.Windows.SystemParameters.VirtualScreenLeft + System.Windows.SystemParameters.VirtualScreenWidth)
        //    {
        //        window.Left = System.Windows.SystemParameters.VirtualScreenWidth + System.Windows.SystemParameters.VirtualScreenLeft - window.Width;
        //    }

        //    if (window.Top + window.Height > System.Windows.SystemParameters.VirtualScreenTop + System.Windows.SystemParameters.VirtualScreenHeight)
        //    {
        //        window.Top = System.Windows.SystemParameters.VirtualScreenHeight + System.Windows.SystemParameters.VirtualScreenTop - window.Height;
        //    }

        //    // Shift window away from taskbar.
        //    {
        //        var taskBarLocation = GetTaskBarLocationPerScreen();

        //        // If taskbar is set to "auto-hide", then this list will be empty, and we will do nothing.
        //        foreach (var taskBar in taskBarLocation)
        //        {
        //            Rectangle windowRect = new Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);

        //            // Keep on shifting the window out of the way.
        //            int avoidInfiniteLoopCounter = 25;
        //            while (windowRect.IntersectsWith(taskBar))
        //            {
        //                avoidInfiniteLoopCounter--;
        //                if (avoidInfiniteLoopCounter == 0)
        //                {
        //                    break;
        //                }

        //                // Our window is covering the task bar. Shift it away.
        //                var intersection = Rectangle.Intersect(taskBar, windowRect);

        //                if (intersection.Width < window.Width
        //                    // This next one is a rare corner case. Handles situation where taskbar is big enough to
        //                    // completely contain the status window.
        //                    || taskBar.Contains(windowRect))
        //                {
        //                    if (taskBar.Left == 0)
        //                    {
        //                        // Task bar is on the left. Push away to the right.
        //                        window.Left = window.Left + intersection.Width;
        //                    }
        //                    else
        //                    {
        //                        // Task bar is on the right. Push away to the left.
        //                        window.Left = window.Left - intersection.Width;
        //                    }
        //                }

        //                if (intersection.Height < window.Height
        //                    // This next one is a rare corner case. Handles situation where taskbar is big enough to
        //                    // completely contain the status window.
        //                    || taskBar.Contains(windowRect))
        //                {
        //                    if (taskBar.Top == 0)
        //                    {
        //                        // Task bar is on the top. Push down.
        //                        window.Top = window.Top + intersection.Height;
        //                    }
        //                    else
        //                    {
        //                        // Task bar is on the bottom. Push up.
        //                        window.Top = window.Top - intersection.Height;
        //                    }
        //                }

        //                windowRect = new Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);
        //            }
        //        }
        //    }
        //}

        // TODO reimplement with win32
        ///// <summary>
        ///// Returned location of taskbar on a per-screen basis, as a rectangle. See:
        ///// https://stackoverflow.com/questions/1264406/how-do-i-get-the-taskbars-position-and-size/36285367#36285367.
        ///// </summary>
        ///// <returns>A list of taskbar locations. If this list is empty, then the taskbar is set to "Auto Hide".</returns>
        //private static List<Rectangle> GetTaskBarLocationPerScreen()
        //{
        //    List<Rectangle> dockedRects = new List<Rectangle>();
        //    foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        //    {
        //        if (screen.Bounds.Equals(screen.WorkingArea) == true)
        //        {
        //            // No taskbar on this screen.
        //            continue;
        //        }

        //        Rectangle rect = new Rectangle();

        //        var leftDockedWidth = Math.Abs((Math.Abs(screen.Bounds.Left) - Math.Abs(screen.WorkingArea.Left)));
        //        var topDockedHeight = Math.Abs((Math.Abs(screen.Bounds.Top) - Math.Abs(screen.WorkingArea.Top)));
        //        var rightDockedWidth = ((screen.Bounds.Width - leftDockedWidth) - screen.WorkingArea.Width);
        //        var bottomDockedHeight = ((screen.Bounds.Height - topDockedHeight) - screen.WorkingArea.Height);
        //        if ((leftDockedWidth > 0))
        //        {
        //            rect.X = screen.Bounds.Left;
        //            rect.Y = screen.Bounds.Top;
        //            rect.Width = leftDockedWidth;
        //            rect.Height = screen.Bounds.Height;
        //        }
        //        else if ((rightDockedWidth > 0))
        //        {
        //            rect.X = screen.WorkingArea.Right;
        //            rect.Y = screen.Bounds.Top;
        //            rect.Width = rightDockedWidth;
        //            rect.Height = screen.Bounds.Height;
        //        }
        //        else if ((topDockedHeight > 0))
        //        {
        //            rect.X = screen.WorkingArea.Left;
        //            rect.Y = screen.Bounds.Top;
        //            rect.Width = screen.WorkingArea.Width;
        //            rect.Height = topDockedHeight;
        //        }
        //        else if ((bottomDockedHeight > 0))
        //        {
        //            rect.X = screen.WorkingArea.Left;
        //            rect.Y = screen.WorkingArea.Bottom;
        //            rect.Width = screen.WorkingArea.Width;
        //            rect.Height = bottomDockedHeight;
        //        }
        //        else
        //        {
        //            // Nothing found!
        //        }

        //        dockedRects.Add(rect);
        //    }

        //    if (dockedRects.Count == 0)
        //    {
        //        // Taskbar is set to "Auto-Hide".
        //    }

        //    return dockedRects;
        //}
    }
}
