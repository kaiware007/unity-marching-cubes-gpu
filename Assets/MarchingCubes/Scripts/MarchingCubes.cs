using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace PavelKouril.MarchingCubesGPU
{
    public class MarchingCubes : MonoBehaviour
    {
        [System.Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Triangle
        {
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;
        }
        
        public int Resolution;
        public Material mat;
        public ComputeShader MarchingCubesCS;

        //public Texture3D DensityTexture { get; set; }
        public Texture DensityTexture;
        
        private int kernelMC;

        private ComputeBuffer appendVertexBuffer;
        private ComputeBuffer argBuffer;

        private void Awake()
        {
            kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");
        }

        private void Start()
        {
            appendVertexBuffer = new ComputeBuffer((Resolution - 1) * (Resolution - 1) * (Resolution - 1) * 5, Marshal.SizeOf<Triangle>(), ComputeBufferType.Append);
            argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);

            MarchingCubesCS.SetInt("_gridSize", Resolution);
            MarchingCubesCS.SetFloat("_isoLevel", 0f);

            MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);
        }

        private void Update()
        {
            MarchingCubesCS.SetTexture(kernelMC, "_densityTexture", DensityTexture);
            appendVertexBuffer.SetCounterValue(0);
            
            MarchingCubesCS.GetKernelThreadGroupSizes(kernelMC, out var numX, out var numY, out var numZ);
            var groupNumX = Mathf.CeilToInt((float) Resolution / numX);
            var groupNumY = Mathf.CeilToInt((float) Resolution / numY);
            var groupNumZ = Mathf.CeilToInt((float) Resolution / numZ);
            
            MarchingCubesCS.Dispatch(kernelMC, groupNumX, groupNumY, groupNumZ);
            //MarchingCubesCS.Dispatch(kernelMC, Resolution / THREAD_SIZE, Resolution / THREAD_SIZE, Resolution / THREAD_SIZE);

            int[] args = new int[] { 0, 1, 0, 0 };
            argBuffer.SetData(args);

            ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

            argBuffer.GetData(args);
            args[0] *= 3;
            argBuffer.SetData(args);

            Debug.Log("Vertex count:" + args[0]);

            mat.SetBuffer("triangles", appendVertexBuffer);
            mat.SetMatrix("model", transform.localToWorldMatrix);

            Graphics.DrawProceduralIndirect(mat, new Bounds(Vector3.zero, Vector3.one), MeshTopology.Triangles,
                argBuffer);
        }

        // private void OnRenderObject()
        // {
        //     mat.SetPass(0);
        //     mat.SetBuffer("triangles", appendVertexBuffer);
        //     mat.SetMatrix("model", transform.localToWorldMatrix);
        //     Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, argBuffer);
        // }

        private void OnDestroy()
        {
            appendVertexBuffer.Release();
            argBuffer.Release();
        }
    }
}