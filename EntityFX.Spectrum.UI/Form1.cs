using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EntityFX.Core.Devices;
using EntityFX.Core.FileReader;
using EntityFX.Core.Platforms.Z48;
using KeyEventArgs = EntityFX.Core.Devices.KeyEventArgs;

namespace EntityFX.Spectrum.UI
{
    public partial class Form1 : Form
    {
        private Timer _timer = new Timer();
        private Z48Spectrum _spectrum;
        private int _counter = 0;
        private Z80Disassembler disassembler;

        public Form1()
        {
            InitializeComponent();
            _timer.Interval = 1;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            _spectrum.Keyboard.ParseKey(true, new KeyEventArgs(e.KeyValue, e.Shift, e.Alt));
        }

        private void Form1_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            _spectrum.Keyboard.ParseKey(false, new KeyEventArgs(e.KeyValue, e.Shift, e.Alt));
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            var pc = _spectrum.ZilogZ80Cpu.Status.PC;
            //var str = disassembler.Disassemble(ref pc);
            //Debug.WriteLine(str);
            _spectrum.Run();
            ++_counter;
            Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _spectrum = new Z48Spectrum();
            disassembler = new Z80Disassembler(_spectrum.Memory);
            _spectrum.LoadROM(@"48.rom");
            _spectrum.Video.OutputBitmap = (Bitmap)spectrumVideoOutputPictureBox.Image;

            var dump = MemoryDumpHelper.DumpAsText(_spectrum.Memory);
            //var diassembly = disassembler.DisassembleAll(_spectrum.Memory);

            byte[] buffer = File.ReadAllBytes(@"C:\Users\SolopiyA\Downloads\cse-code-7\cse-code-7\roms\POPCORN.Z80");
            Format2 fileFormatz80 = new Format2();
            fileFormatz80.Read(buffer);
            _spectrum.LoadRAMFromFile(fileFormatz80);
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }
    }
}
