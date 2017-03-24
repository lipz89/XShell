using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace XShell
{
    class Shell
    {
        private static byte[] GetBuilder()
        {
            //根据资源名称从Assembly中获取此资源的Stream
            var builderPath = "XBuilder.exe";
            if (File.Exists(builderPath))
            {
                Stream stream = File.OpenRead(builderPath);
                var buffer = StreamToBytes(stream);
                return buffer;
            }
            else
            {
                return null;
            }
        }

        /// <summary> 
        /// 将 Stream 转成 byte[] 
        /// </summary> 
        private static byte[] StreamToBytes(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        internal static void Press(string path)
        {
            var array1 = GetBuilder();
            if (array1 != null)
            {
                var array2 = GetAssemblyToByte(path);
                Save(path, array1, array2);
            }
            else
            {
                MessageBox.Show("没找到外壳程序。");
            }
        }
        /// <summary>
        /// 把指定程序集转化为二进制流
        /// </summary>
        /// <param name="assemblyFileName">路径程序集</param>
        /// <returns>二进制源</returns>
        public static byte[] GetAssemblyToByte(string assemblyFileName)
        {
            byte[] assemblySource = null;
            if (File.Exists(assemblyFileName))
            {
                FileStream fStream = new FileStream(assemblyFileName, FileMode.Open);
                BinaryReader bReader = new BinaryReader(fStream);
                assemblySource = bReader.ReadBytes(Convert.ToInt32(fStream.Length));
                fStream.Close();
                bReader.Close();
            }
            return assemblySource;
        }

        private static void Save(string path, byte[] buffer, byte[] innerBuffer)
        {
            var str = path.Replace(".exe", ".Outer.exe");
            using (FileStream fStream = File.Create(str))
            {
                using (BinaryWriter bReader = new BinaryWriter(fStream))
                {
                    bReader.Write(buffer);
                    bReader.Write(mz(innerBuffer));

                    fStream.Seek(64, SeekOrigin.Begin);
                    bReader.Write(buffer.Length);
                    bReader.Write(innerBuffer.Length);
                }
            }
        }
        /// <summary>
        /// 还原
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static byte[] lz(byte[] buffer)
        {
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
        /// <summary>
        /// 一个简单的压缩及混淆算法
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private static byte[] mz(byte[] buffer)
        {
            //return buffer;
            var list = new List<byte>();
            var l = (ulong)buffer.Length;
            while (l > 0 || list.Count < 8)
            {
                if (l > 0)
                {
                    var a = l & 0xff;
                    l = l >> 8;
                    list.Add((byte)a);
                }
                else
                {
                    list.Add(0);
                }
            }

            for (int i = 0; i < buffer.Length; i++)
            {
                var c = see(buffer, i);
                if (c > 1)
                {
                    var cc = c;
                    while (cc > 255)
                    {
                        cc = cc - 255;
                        list.Add(lc(buffer[i]));
                        list.Add(lc(buffer[i]));
                        list.Add(255);
                        list.Add(255);
                    }
                    if (cc > 0)
                    {
                        list.Add(lc(buffer[i]));
                        list.Add(lc(buffer[i]));
                        list.Add((byte)cc);
                        list.Add((byte)cc);
                    }
                    i += c - 1;
                }
                else
                {
                    list.Add(lc(buffer[i]));
                }
            }

            return list.ToArray();
        }

        private static byte lc(byte bit)
        {
            if (bit >= 128)
                return (byte)(bit - 128);
            return (byte)(bit + 128);
        }

        private static int see(byte[] buffer, int index)
        {
            var l = index + 1;
            while (l < buffer.Length && buffer[l] == buffer[index])
            {
                l++;
            }
            return l - index;
        }
    }
}