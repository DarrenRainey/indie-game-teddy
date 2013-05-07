using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Prototype2
{
    class CreditsLine
    {
        private String text;
        public Vector2 position;
        public bool active;
        private float r;
        private float g;
        private float b;
        private float alpha;  

        public float R
        {
            get { return r; }
            set { r = value; }
        }

        public float G
        {
            get { return g; }
            set { g = value; }
        }

        public float B
        {
            get { return b; }
            set { b = value; }
        }   

        public String Text
        {
            get { return text; }
            set { text = value; }
        }        

        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }        
                
        public CreditsLine(String text)
        {
            this.text = text;
            this.position = Vector2.Zero;            
            this.r = 1f;
            this.g = 1f;
            this.b = 1f;
            this.alpha = 0f;
            this.active = false;
        }

        public CreditsLine(String text, float r, float g, float b)
        {
            this.text = text;
            this.position = Vector2.Zero;
            this.r = r;
            this.g = g;
            this.b = b;
            this.alpha = 0f;
            this.active = false;
        }
    }
}
