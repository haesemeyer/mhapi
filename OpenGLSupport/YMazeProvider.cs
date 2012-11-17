using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.Enumerations;

using ipp;

namespace MHApi.OpenGLSupport
{
    public class YMazeProvider : TwoDProviderBase
    {
        private Quad _arm1;

        private Quad _arm2;

        private Quad _arm3;

        private FColor _vertex13;

        private FColor _vertex32;

        private FColor _vertex21;

        /// <summary>
        /// Creates a new Y-Maze provider. The coordinates are intended to follow each other in clock-wise direction starting at the meeting point
        /// of bottom arm and left arm
        /// </summary>
        public YMazeProvider(IppiPoint_32f p1, IppiPoint_32f p2, IppiPoint_32f p3, IppiPoint_32f p4, IppiPoint_32f p5, IppiPoint_32f p6, IppiPoint_32f p7, IppiPoint_32f p8, IppiPoint_32f p9, FColor arm1, FColor arm2, FColor arm3)
        {
            _arm1 = new Quad(p1, p2, p3, p4, arm1);
            _arm2 = new Quad(p4, p5, p6, p7, arm2);
            _arm3 = new Quad(p7, p8, p9, p1, arm3);
            _vertex13 = new FColor((arm1.R + arm3.R) / 2, (arm1.G + arm3.G) / 2, (arm1.B + arm3.B) / 2);
            _vertex21 = new FColor((arm1.R + arm2.R) / 2, (arm1.G + arm2.G) / 2, (arm1.B + arm2.B) / 2);
            _vertex32 = new FColor((arm2.R + arm3.R) / 2, (arm2.G + arm3.G) / 2, (arm2.B + arm3.B) / 2);
        }

        public override void Draw(SharpGL.OpenGL gl)
        {
            //Process alignment in baseclass
            base.Draw(gl);

            //Draw arms
            _arm1.Draw(gl);
            _arm2.Draw(gl);
            _arm3.Draw(gl);

            //Draw middle triangle. At each vertex of this triangle two arms meet.
            //So for color merging we will give each vertex the average color of the two
            //adjacent arms - precalculated in constructor

            gl.Begin(SharpGL.Enumerations.BeginMode.Triangles);

            gl.Color(_vertex32.R, _vertex32.G, _vertex32.B);
            gl.Vertex(_arm3.Corner1.x, _arm3.Corner1.y);

            gl.Color(_vertex21.R, _vertex21.G, _vertex21.B);
            gl.Vertex(_arm1.Corner4.x, _arm1.Corner4.y);

            gl.Color(_vertex13.R, _vertex13.G, _vertex13.B);
            gl.Vertex(_arm1.Corner1.x, _arm1.Corner1.y);

           

            

            gl.End();
        }




        public override string ToFileString()
        {
            throw new NotImplementedException();
        }

        public override void FromFileString(string s)
        {
            throw new NotImplementedException();
        }
    }
}
