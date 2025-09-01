using ComputeSharp;

namespace Render.GpuShaders
{
    [EmbeddedBytecode(DispatchAxis.X)]
    public readonly partial struct ClearIntKernel : IComputeShader
    {
        public readonly ReadWriteBuffer<int> Buffer;

        public ClearIntKernel(ReadWriteBuffer<int> buffer)
        {
            Buffer = buffer;
        }

        public void Execute()
        {
            Buffer[ThreadIds.X] = 0;
        }
    }
}
