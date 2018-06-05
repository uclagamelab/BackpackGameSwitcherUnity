// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*

This shader blurs an image, it *will* workd on a sprite,
but it works best if you use your image as a texture on 
a quad.  On sprites, the blur won't extend into empty
parts of the image, and looks bad.

I had to change a bunch of stuff to be hardcoded (for performance reasons)
but if you go back some versions in git, you can find a more flexible,
fancier version of this shader, (but it won't run well on the surface)

To get best results, 
	- enable mipmaps for your image
	- set the filter mode to trilinear (otherwise, will get strange jumps when varying blur distance)
        
		
        _MainTex 
			The image texture
		
		_Color
			The tint color, also used to fade image out.

		_BlurDist
			for each pixel, how far away to sample
			(extremeness of blur)
		
		_MipsBias
			Bigger value means use a lower-sampled version
			of the image (helps fixed weird blocky glitches)
			akin to downsample in builtin in image effect blur.
		
		_BlurAmt 
			Not really necessary anymore, better to set blur dist to 0
			but the mixture of
			the unblurred image and the blurred image
			0 = no blurred image
			.5 = average of blurred and blurred
			1 = all blurred


*/

 Shader "Fujitsu/SpriteBlurredBetter"
 {  
     Properties
     {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0,0,0,1)
		//_BluIters ("Blur Iterations",  Range (0, 12)) = 2
		_BlurDist ("Blur Distance",  float) = 1
		_MipsBias ("Downsample",  float) = 0
		_BlurAmt ("Amount", Range(0,1)) = 1
     }
     SubShader
     {
         Tags 
         { 
             "RenderType" = "Transparent" 
             "Queue" = "Transparent+1" 
         }
 
         Pass
         {
             ZWrite Off
             Blend SrcAlpha OneMinusSrcAlpha 
			 AlphaTest Off
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile DUMMY PIXELSNAP_ON
  
             sampler2D _MainTex;
             float4 _Color;
			 float  _BluIters;
			 float4 _MainTex_TexelSize;
			 float _BlurDist;
			 float _MipsBias;
			 float _BlurAmt;

             struct Vertex
             {
                 float4 vertex : POSITION;
                 float2 uv_MainTex : TEXCOORD0;
                 float2 uv2 : TEXCOORD1;
             };
     
             struct Fragment
             {
                 float4  vertex : POSITION;
                 float2 uv_MainTex : TEXCOORD0;
                 float2 uv2 : TEXCOORD1;
             };
  
             Fragment vert(Vertex v)
             {
                 Fragment o;
     
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.uv_MainTex = v.uv_MainTex;
                 o.uv2 = v.uv2;
     
                 return o;
             }
                                                     
             float4 frag(Fragment IN) : COLOR
             {
			 
			 
				 float dx = _MainTex_TexelSize[0];
				 float dy = _MainTex_TexelSize[1];


				/**/
                 float4 o = float4(1, 1, 1, 1);
 
                 float4 cOrig = tex2D (_MainTex, IN.uv_MainTex);
			
				if ( _BlurAmt == 0 || _BlurDist == 0)
				{
					o.rgba = cOrig;
					o.rgba = o.rgba * _Color;
					return o;
				}
				else
				{
				 half4 c = cOrig;
				 
			

				 //float itersSmoothAmt = _BluIters % 1;

				 //temporarily hardcoding blur iterations.
				 int blurRad = 2;//(int) ceil(_BluIters);
				
				 
				  c =  0;

				  float divisorForWeights = 0;
				   float outerDivisorForWeights = 0;

				  float4 outerC = 0;

				 for (int i = -blurRad; i <= blurRad; i++)
				 {
				 for (int j = -blurRad; j <= blurRad; j++)
				 {
					float4 ofs;
				 	ofs[0] = IN.uv_MainTex[0] + (i) * dx * _BlurDist;
					ofs[1] = IN.uv_MainTex[1] + (j) * dy * _BlurDist;
					ofs[2] = 0;
					ofs[3] = _MipsBias;//(1 + _BlurDist * .25);

					//
					float coeff = (1 + 2*blurRad - abs(i) - abs(j)); // manhattanDistance
					// sqrt(2) * blurRad*blurRad - (i*i + j*j); // distance squared

				
						//add in gaussian(?)
						c += coeff * tex2Dbias(_MainTex, ofs); 
						//divisorForWeights += coeff;
					

					
					
				 }
				 }
                 
				 c = (c ) / 66;////(divisorForWeights + itersSmoothAmt*outerDivisorForWeights);


				 o.rgba = (_BlurAmt) * c + (1 - _BlurAmt) * cOrig;
				 o.rgba = o.rgba * _Color;
     
                     
                 return o;
				}

				
             }
 
             ENDCG
         }

     }
 }