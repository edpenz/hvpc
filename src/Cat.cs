public class Cat
{
    public static void DualCat(Stream aSrc, Stream aDst, Stream bSrc, Stream bDst)
    {
        var aCopy = aSrc.CopyToAsync(aDst);
        var bCopy = bSrc.CopyToAsync(bDst);

        while (!aCopy.IsFaulted && !bCopy.IsFaulted)
        {
            var waitResult = Task.WaitAny(new Task[] { aCopy, bCopy });

            if (aCopy.IsCompletedSuccessfully)
            {
                aCopy = aSrc.CopyToAsync(aDst);
            }
            else
            {
                return;
            }

            if (bCopy.IsCompletedSuccessfully)
            {
                bCopy = bSrc.CopyToAsync(bDst);
            }
            else
            {
                return;
            }
        }
    }
}
