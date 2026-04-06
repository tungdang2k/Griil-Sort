// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Vpcw6aXtB7ve3VMnof+SitNJCO7BB6VGxbH4aRlJaEfdfoaI6PB13SMtQFItv2ZX9K3RL01IvyU7Yk3gYBHMt0F2VAk9+NrBIf9jPTGrx79TVzGTrd35n/wgiVNiw5ahBCl9UPuHfqqRcyvZwzna0SwL7Tl/XqYbGafKGMIJCmA5cRcdFIFqxhUn3P2CMLOQgr+0u5g0+jRFv7Ozs7eyse1Cry3mczFFGBsUpfwNsqNeK+inBbqf1InwZ5Px2HcwcsTzNZ8IebBkNdy6oq4Foy3cr1zSnR4GvBqqJTCzvbKCMLO4sDCzs7IIjYQTMg4BFVgFVSnCGl5AX/sIu2m95OkeQuTWrRbAbYhiAh9frT6SM6XV5mrD+pk5t6YnV/SyVbCxs7Kz");
        private static int[] order = new int[] { 5,6,3,9,12,9,11,9,12,9,12,11,13,13,14 };
        private static int key = 178;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
