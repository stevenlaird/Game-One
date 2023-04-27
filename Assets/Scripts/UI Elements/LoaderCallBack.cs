using UnityEngine;

public class LoaderCallBack : MonoBehaviour
{
    private bool isFirstUpdate = true;  // Boolean to ensure the method is called only once

    ///////////////////

    private void Update()
    {
        if (isFirstUpdate)              // if this is the first time Update is called
        {
            isFirstUpdate = false;          // set isFirstUpdate to false to prevent future calls
            Loader.LoaderCallBack();        // call the LoaderCallBack method from the Loader class
        }
    }
}