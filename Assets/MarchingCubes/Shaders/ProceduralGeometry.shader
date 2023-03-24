Shader "PavelKouril/Marching Cubes/Procedural Geometry"
{
	SubShader
	{
		Cull Front

		Pass
		{
			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct Vertex
			{
				float3 vPosition;
			};

			struct Triangle
			{
				Vertex v[3];
			};

			uniform StructuredBuffer<Triangle> triangles;
			uniform float4x4 model;

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			v2f vert(uint id : SV_VertexID)
			{
				uint pid = id / 3;
				uint vid = id % 3;

				v2f o;
				// o.vertex = mul(UNITY_MATRIX_VP, mul(model, float4(triangles[pid].v[vid].vPosition, 1)));
				// o.normal = mul(unity_ObjectToWorld, triangles[pid].v[vid].vNormal);
				o.vertex = float4(triangles[pid].v[vid].vPosition, 1);
				//o.normal = triangles[pid].v[vid].vNormal;
				o.normal = 0;
				return o;
			}

			[maxvertexcount(3)] //出力する頂点の最大数　正直よくわからない
            void geom(triangle v2f input[3], inout TriangleStream<v2f> stream)
            {
                // 法線を計算
                float3 vec1 = input[0].vertex - input[1].vertex;
                float3 vec2 = input[0].vertex - input[2].vertex;
                float3 normal = -normalize(cross(vec1, vec2));

                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    v2f v = input[i];
                    v2f o;
                    o.vertex = mul(UNITY_MATRIX_VP, mul(model, v.vertex));
                	o.normal = mul(unity_ObjectToWorld, normal);
                    stream.Append(o);
                }
            }
			
			float4 frag(v2f i) : SV_Target
			{
				//return float4(i.normal,1);
				float d = max(dot(normalize(_WorldSpaceLightPos0.xyz), i.normal), 0);
				return float4(d,d,d, 1);
			}
			ENDCG
		}
	}
}
