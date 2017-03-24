using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace XBuilder
{
    internal class Program
    {
        private static Assembly mp;

        [STAThread]
        private static int Main(string[] args)
        {
            byte[] rawAssembly;
            int result;
            if (!lf(Application.ExecutablePath, out rawAssembly))
            {
                MessageBox.Show("文件不可用。", "XBuilder");
                result = -1;
            }
            else
            {
                int num;
                try
                {
                    mp = Assembly.Load(rawAssembly);
                    MethodInfo entryPoint = mp.EntryPoint;
                    object[] parameters = null;
                    if (entryPoint.GetParameters().Length > 0)
                    {
                        parameters = new object[]
                        {
                            args
                        };
                    }
                    object obj = entryPoint.Invoke(null, parameters);
                    if (obj != null)
                    {
                        num = (int)obj;
                    }
                    else
                    {
                        num = 0;
                    }
                }
                catch
                {
                    num = -1;
                }
                result = num;
            }
            return result;
        }

        private static bool lf(string executablePath, out byte[] rawAssembly)
        {
            try
            {
                FileStream fileStream = new FileStream(executablePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                int length = (int)fileStream.Length;//总长度
                fileStream.Seek(64, SeekOrigin.Begin);
                BinaryReader binaryReader = new BinaryReader(fileStream);

                var len = binaryReader.ReadInt32();//壳长度
                var innerLength = length - len;
                fileStream.Seek(len, SeekOrigin.Begin);
                byte[] array = new byte[innerLength];
                fileStream.Read(array, 0, innerLength);
                fileStream.Close();
                rawAssembly = lz(array);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            rawAssembly = null;
            return false;
        }
        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static byte[] lz(byte[] buffer)
        {
            //return buffer;
            var i = 7;
            ulong l = 0;
            while (i >= 0)
            {
                var k = buffer[i];
                l = (l + k);
                if (i > 0)
                    l = l << 8;
                i--;
            }
            var bytes = new byte[l];
            i = 8;
            var z = 0;
            while (i < buffer.Length)
            {
                if (buffer.Length > i + 3 && buffer[i] == buffer[i + 1] && buffer[i + 2] == buffer[i + 3])
                {
                    for (int j = 0; j < buffer[i + 2]; j++)
                    {
                        bytes[z++] = lc(buffer[i]);
                    }
                    i += 4;
                }
                else
                {
                    bytes[z++] = lc(buffer[i]);
                    i++;
                }
            }
            return bytes;
        }
        private static byte lc(byte bit)
        {
            if (bit >= 128)
                return (byte)(bit - 128);
            return (byte)(bit + 128);
        }
    }
}
