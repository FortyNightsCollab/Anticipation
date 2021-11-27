
public class CenteredSelection
{
    int[] numDirections;
   
    public CenteredSelection(int N, int NE, int E, int SE, int S, int SW, int W, int NW)
    {
        numDirections = new int[8];
        numDirections[0] = N;
        numDirections[1] = NE;
        numDirections[2] = E;
        numDirections[3] = SE;
        numDirections[4] = S;
        numDirections[5] = SW;
        numDirections[6] = W;
        numDirections[7] = NW;
    }

    public CenteredSelection(int n)
    {
        numDirections = new int[8];

        for(int i = 0; i < numDirections.Length; i++)
        {
            numDirections[i] = n;
        }
    }

    public int[] GetDirections()
    {
        int[] arrayToReturn = new int[numDirections.Length];

        for(int i = 0; i < arrayToReturn.Length; i++)
        {
            arrayToReturn[i] = numDirections[i];
        }
        return arrayToReturn;
    }
}
