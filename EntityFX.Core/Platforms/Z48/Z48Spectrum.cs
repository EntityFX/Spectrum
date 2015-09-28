using System;
using System.IO;
using EntityFX.Core.Common;
using EntityFX.Core.Common.Cpu;
using EntityFX.Core.CPU;
using EntityFX.Core.CPU.Registers;
using EntityFX.Core.Devices;
using EntityFX.Core.FileReader;

namespace EntityFX.Core.Platforms.Z48
{
    public class Z48Spectrum
    {
        private readonly IZilogZ80Cpu _zilogZ80Cpu;
        private IMemory _memory;
        private IVideoOutput _video;
        private IKeyboardInput _keyboard;
        private Z48IO _inputOutputDevice;

        private bool _isRunning;

        public Z48Spectrum()
        {
            _memory = new Z48Memory();
            var registerFile = new RegisterFile();
            var lookupTables = new LookupTables();
            var cpuStack = new CpuStack(_memory, registerFile);
            _keyboard = new Keyboard();
            _video = new Video(_memory);
            var outputDevice = (IOutputDevice)_video;
            var inputDevice = (IInputDevice) _keyboard;
            _inputOutputDevice = new Z48IO(inputDevice, outputDevice) { Keyboard = inputDevice, Video = outputDevice };
            var alu = new Alu(_memory, registerFile, cpuStack, lookupTables);
            var executionUnit = new ExecutionUnit(_memory, registerFile, cpuStack, alu, _inputOutputDevice, lookupTables);
            _zilogZ80Cpu = new ZilogZ80Cpu(_memory, _inputOutputDevice, new CpuStack(_memory, registerFile), lookupTables, executionUnit, registerFile);
        }

        public void Run()
        {
            _zilogZ80Cpu.Execute();
            _video.Flash();
            _zilogZ80Cpu.States -= ExecutionUnit.EventNextEvent;
            _zilogZ80Cpu.Interrupt();
        }

        void Reset()
        {
            _zilogZ80Cpu.Reset();
        }

        /// <summary>
        /// Load a ROM file from address 0x0000 to address 0x3FFF
        /// </summary>
        public void LoadROM(string filePath)
        {

            Stream stream = new FileStream(filePath, FileMode.Open);
            if (stream.Length != 0x4000)
                throw new Exception("Invalid file size.");
            stream.Read(_memory.Raw, 0x0000, (int)stream.Length);
            stream.Close();
        }

        public void LoadRAM(byte[] buffer)
        {
            buffer.CopyTo(_memory.Raw, 0x4000);
        }

        public void LoadRAMFromFile(Format2 format)
        {
            _zilogZ80Cpu.Reset();
            if (format.FileVersion == 1)
                LoadRAM(format.RAM);
            else
            {
                LoadPage(format.Pages[4], 0x8000);
                LoadPage(format.Pages[5], 0xC000);
                LoadPage(format.Pages[8], 0x4000);
            }

            _zilogZ80Cpu.Status = format.Status;
            _video.Refresh();
        }


        public void LoadPage(byte[] page, int offset)
        {
            Array.Copy(page, 0, _memory.Raw, offset, page.Length);
        }

        public IMemory Memory
        {
            get { return _memory; }
        }

        public IVideoOutput Video
        {
            get { return _video; }
        }

        public IKeyboardInput Keyboard
        {
            get { return _keyboard; }
        }

        public IInputOutputDevice IO
        {
            get { return _inputOutputDevice; }
        }

        public IZilogZ80Cpu ZilogZ80Cpu
        {
            get { return _zilogZ80Cpu; }
        }
    }

}