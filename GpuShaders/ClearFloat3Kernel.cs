using ComputeSharp;
using System.Numerics;

namespace Render.GpuShaders
{
    [EmbeddedBytecode(DispatchAxis.X)]
    public readonly partial struct ClearFloat3Kernel : IComputeShader
    {
        public readonly ReadWriteBuffer<float3> Buffer;

        public ClearFloat3Kernel(ReadWriteBuffer<float3> buffer)
        {
            Buffer = buffer;
        }

        public void Execute()
        {
            Buffer[ThreadIds.X] = new float3(0f, 0f, 0f);
        }
    }
}
