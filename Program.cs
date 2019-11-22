using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Teste
{
    class Program
    {
        static void Main(string[] args)
        {
            new Comunicacao().Iniciar();
        }
    }

    class Comunicacao
    {
        SerialPort _serialPort;
        public Comunicacao()
        {
            var baudRate = 5787;
            var portName = "/dev/ttyAMA0";
            _serialPort = new SerialPort()
            {
                //porta para se comunicar com a MDH
                PortName = portName,      // assign the port name 
                BaudRate = baudRate,                // Baudrate = 9600bps
                Parity = Parity.Even,           // Parity bits = none  
                DataBits = 8,                   // No of Data bits = 8
                StopBits = StopBits.One,        // No of Stop bits = 1
                ReadTimeout = -1,
                Handshake = Handshake.None
            };
            Console.WriteLine("Baudrate {0}", _serialPort.BaudRate);
            Console.WriteLine("Encoding {0}", _serialPort.Encoding);
        }

        public void Iniciar()
        {
            while (true)
            {
                Console.WriteLine("Digite o cmd");
                var cmd = Console.ReadLine().Trim();
                if (!string.IsNullOrWhiteSpace(cmd))
                {
                    var arr = cmd.Split('-');
                    var lstByte = new List<byte>();
                    foreach (var item in arr)
                        lstByte.Add(Convert.ToByte(item, 16));
                    //array = new byte[1] { Convert.ToByte(cmd, 16) };
                    var dadosRecebidos = new byte[0];
                    EnviarPacotes(lstByte.ToArray());
                    ReceberDados(out dadosRecebidos);
                }
            }
        }


        public bool EnviarPacotes(byte[] comando)
        {
            Console.WriteLine("Enviado {0}", ByteToHex(comando));
            try
            {
                if (!_serialPort.IsOpen)
                    _serialPort.Open();
                //_serialPort.BaudRate = 5787;
                //_serialPort.Parity = Parity.Even;
                //_serialPort.WriteLine("PAUL");
                foreach (var item in comando)
                {
                    var arr = new byte[1] { item };
                    _serialPort.Write(arr, 0, arr.Length);
                    Thread.Sleep(5);    
                }
                //_serialPort.Write(comando, 0, comando.Length);
                _serialPort.DiscardInBuffer();
                return true;
            }
            catch (Exception ex) // catch (Exception e)
            {
                Console.WriteLine("Err {0}", ex.Message);
                Console.WriteLine("Porta {0}", _serialPort.PortName);
            }
            return false;
        }

        private bool ReceberDados(out byte[] dadosRecebidos)
        {
            dadosRecebidos = new byte[0];
            var lstByte = new List<byte>();
            var qtd = 0;
            var bytes = _serialPort.BytesToRead;
            var sbr = new StringBuilder();
            do
            {
                for (var i = 0; i < bytes; i++)
                    lstByte.Add((byte)_serialPort.ReadByte());
                qtd += bytes;
                Thread.Sleep(20);
            }
            while ((bytes = _serialPort.BytesToRead) > 0);
            _serialPort.DiscardOutBuffer();
            if (qtd == 0)
                return false;
            Console.WriteLine("Recebido {0}", ByteToHex(lstByte.ToArray()));
            return true;
        }

        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            var builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (var data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0'));
            //return the converted value
            return builder.ToString().ToUpper();
        }
    }
}
