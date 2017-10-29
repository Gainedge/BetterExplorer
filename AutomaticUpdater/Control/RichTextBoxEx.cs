using System.Text;
using System.Windows.Forms;

namespace wyDay.Controls
{
    internal delegate void LinkHandler(object sender, string linkTarget);

	internal class RichTextBoxEx : RichTextBox
	{
		public RichTextBoxEx()
		{
            DetectUrls = false;
            BorderStyle = BorderStyle.None;
		}

        //create the RichTextBox with a thin border
        protected override CreateParams CreateParams
        {
            get
            {
                //const int WS_BORDER = 0x00800000;
                //const int WS_EX_STATICEDGE = 0x00020000;

                CreateParams cp = base.CreateParams;

                //add the thin 'Static Edge"
                cp.ExStyle |= 0x00020000;

                return cp;
            }
        }

#if CLIENT
	    private string defaultRTFHeader;
        public string SterilizeRTF(string text)
        {
            // if defaultRTF header = null, load it
            if (defaultRTFHeader == null)
                defaultRTFHeader = Rtf;

            StringBuilder sb = new StringBuilder();

            // get the font table of the default header

            int startIndex = defaultRTFHeader.IndexOf("\\fonttbl") + 8;
            int indexOn = startIndex;
            int numOpenBraces = 0;

            do
            {
                if (defaultRTFHeader[indexOn] == '{')
                    numOpenBraces++;
                else if (defaultRTFHeader[indexOn] == '}')
                    numOpenBraces--;

                indexOn++;

            } while (numOpenBraces > -1);


            string defaultFontTable = defaultRTFHeader.Substring(startIndex, indexOn - startIndex);

            // find the font table of the input text
            indexOn = startIndex = text.IndexOf("\\fonttbl") + 8;

            // add the header from the source text
            sb.Append(text.Substring(0, startIndex));

            // add the font table from the default header
            sb.Append(defaultFontTable);


            // find the end of the source text's fontTable
            numOpenBraces = 0;

            do
            {
                if (text[indexOn] == '{')
                    numOpenBraces++;
                else if (text[indexOn] == '}')
                    numOpenBraces--;

                indexOn++;

            } while (numOpenBraces > -1);


            text = text.Substring(indexOn, text.Length - indexOn);


            // ----- Remove other font sizes

            bool atLeaseOneDigit = false;

            startIndex = 0;

            do
            {
                indexOn = startIndex = defaultRTFHeader.IndexOf("\\fs", startIndex) + 3;

                while (char.IsDigit(defaultRTFHeader, indexOn))
                {
                    indexOn++;
                    atLeaseOneDigit = true;
                }

            } while (!atLeaseOneDigit);

            string defaultFontSize = defaultRTFHeader.Substring(startIndex, indexOn - startIndex);


            indexOn = 0;

            do
            {
                // remove all other font sizes other than "\fs<N>" other than the default "fsM"

                // number of slashes preceding "\fs<N>"
                int numSlashesPreceding = 0;

                // find a "\fs" element
                int slashIndex = text.IndexOf("\\fs", indexOn);
                indexOn = startIndex = slashIndex + 3;

                // count how many slashes precede the "\fs<N>" element
                while (slashIndex > 1 && text[--slashIndex] == '\\')
                {
                    numSlashesPreceding++;
                }
                
                // if it's an even number of slashes (0,2,...) then we're dealing with a "REAL"
                // "\fs<N>" element instead of just escaped slashes followed by the text "fs"

                if (startIndex > 2 && numSlashesPreceding % 2 == 0)
                {
                    sb.Append(text.Substring(0, startIndex));
                    sb.Append(defaultFontSize);

                    while (char.IsDigit(text, startIndex))
                        startIndex++;

                    text = text.Substring(startIndex, text.Length - startIndex);

                    indexOn = 0;
                }

            } while (startIndex > 2); // -1 + 3 = 2 (that is, no more "\fs" found)


            // add the left over text
            sb.Append(text);

            // set the RTF text
            return sb.ToString();
        }
#endif
	}
}
