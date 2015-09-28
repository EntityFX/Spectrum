using System;
using System.Diagnostics;
using EntityFX.Core.Common;
using EntityFX.Core.Common.Cpu;
using EntityFX.Core.Common.Cpu.Registers;
using EntityFX.Core.CPU.Registers;

namespace EntityFX.Core.CPU
{
    public class ZilogZ80Cpu : IZilogZ80Cpu
    {
        // Memory and IO access
        private readonly IMemory _memory;
        private readonly IInputOutputDevice _io;
        private readonly IExecutionUnit _executionUnit;
        private readonly ICpuStack _cpuStack;

        private IRegisterFile _registerFile;


        // By default fetching won't be stopped
        //private int _StatementsToFetch = -1;

        /// <summary>
        /// Fetch event (used during debug)
        /// </summary>
        public event OnFetchHandler OnFetch
        {
            add { _executionUnit.OnFetch += value; }
            remove { _executionUnit.OnFetch -= value; }
        }


        /// <summary>
        /// Create new system based on Z80
        /// </summary>
        /// <param name="memory">System memory</param>
        /// <param name="io">IO</param>
        /// <param name="cpuStack"></param>
        /// <param name="lookupTables"></param>
        /// <param name="executionUnit"></param>
        /// <param name="registerFile"></param>
        public ZilogZ80Cpu(IMemory memory, IInputOutputDevice io, ICpuStack cpuStack, ILookupTables lookupTables, IExecutionUnit executionUnit, IRegisterFile registerFile)
        {
            _memory = memory;
            _io = io;
            _executionUnit = executionUnit;
            _registerFile = registerFile;
            _cpuStack = cpuStack;
            lookupTables.Init();
            Reset();
        }



        /// <summary>
        /// System memory
        /// </summary>
        public IMemory Memory
        {
            get
            {
                return _memory;
            }
        }

        /// <summary>
        /// System IO
        /// </summary>
        public IInputOutputDevice IO
        {
            get
            {
                return _io;
            }
        }

        public void Execute()
        {
            _executionUnit.Execute();
        }

        public void Interrupt()
        {
            // Check if the interrupts are enabled
            if (_registerFile.IFF1)
            {

                // The Z80 is no more halted
                _registerFile.Halted = false;

                // Reset Interrupt Flip Flops
                // When the CPU accepts a maskable interrupt, both IFF1 and IFF2 are automatically reset,
                // inhibiting further interrupts until the programmer issues a new El instruction.
                _registerFile.IFF1 = false;
                _registerFile.IFF2 = false;


                // Push program counter
                _cpuStack.Push(_registerFile.PC);

                switch (_registerFile.IM)
                {
                    case 0:
                        _registerFile.PC = 0x0038;
                        _executionUnit.CpuTicks += 12;
                        break;
                    case 1:
                        _registerFile.PC = 0x0038;
                        _executionUnit.CpuTicks += 13;
                        break;
                    case 2:
                        var interruptTableAddress = (ushort)((_registerFile.I << 8) | 0xFF);
                        _registerFile.PC = _memory.ReadWord(interruptTableAddress);
                        _executionUnit.CpuTicks += 19;
                        break;
                    default:
                        Debug.Fail(String.Format("Unknown interrupt mode {0}\n", _registerFile.IM));
                        break;
                }
            }
        }

        /// <summary>
        /// Z80 internal status
        /// </summary>
        public IRegisterFile Status
        {
            get
            {
                return _registerFile;
            }
            set { _registerFile = value; }
        }

        public int States
        {
            get { return _executionUnit.CpuTicks; }
            set { _executionUnit.CpuTicks = value; }
        }


        /// <summary>
        /// Resets the system
        /// </summary>
        public void Reset()
        {
            _registerFile.Reset();
        }

        /// <summary>
        /// Number of statements to fetch before returning (-1) if no return at all.
        /// It should be used for debug purpose (set to -1 after each fetch).
        /// </summary>
        public int StatementsToFetch
        {
            get
            {
                return _executionUnit.StatementsToFetch;
            }
            set
            {
                _executionUnit.StatementsToFetch = value;
            }
        }

    }
}