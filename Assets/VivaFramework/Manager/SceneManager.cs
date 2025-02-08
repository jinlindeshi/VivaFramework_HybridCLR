using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace VivaFramework
{
	
	public class SceneManager:MonoBehaviour
    {
	    
	    public void LoadSceneAsync(string sceneName, Action<Scene> callBack, LoadSceneMode mode)
	    {

		    print("SceneManager - LoadSceneAsync " + sceneName);
		    string mainAbName = "scene_" + sceneName.ToLower();
		    Main.resManager.LoadAssetBundle(mainAbName, true, () =>
		    {
			    StartCoroutine(LoadingScene(sceneName, callBack, mode));
		    });
		}
	    
	    public void UnLoadSceneAsync(Scene scene, Action callBack)
	    {
		    StartCoroutine(UnLoadingScene(scene, callBack));
	    }


		IEnumerator LoadingScene(string sceneName, Action<Scene> callBack, LoadSceneMode mode)
		{
			AsyncOperation sceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
			yield return sceneLoad;

			Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
			UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
			if (callBack != null)
			{
				// Debug.Log("你妹啊~" + s);
				callBack(s);
			}
		}
	    
	    IEnumerator UnLoadingScene(Scene scene, Action callBack)
	    {
		    AsyncOperation sceneUnload = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
		    yield return sceneUnload;

		    if (callBack != null)
		    {
			    callBack();
		    }
	    }
    }
}
