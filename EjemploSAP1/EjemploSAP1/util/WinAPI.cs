using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EjemploSAP1.util
{
    class WinAPI
    {

        const int SWP_NOSIZE = 0x1;
        const int SWP_NOMOVE = 0x2;
        const int SWP_NOACTIVATE = 0x10;
        const int wFlags = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;
        const int HWND_TOPMOST = -1;
        const int HWND_NOTOPMOST = -2;
        
        [DllImport("user32.DLL")]
        private extern static void SetWindowPos(
            int hWnd, int hWndInsertAfter,
            int X, int Y,
            int cx, int cy,
            int wFlags);

        public static void SiempreEncima(int handle)
        {
            SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, wFlags);
        }

        public static void NoSiempreEncima(int handle)
        {
            SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, wFlags);

        }
    }
}
