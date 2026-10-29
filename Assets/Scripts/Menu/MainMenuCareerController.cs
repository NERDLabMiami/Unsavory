using UnityEngine;

public class MainMenuCareerController : MonoBehaviour
{
    public void StartNewCareer()
    {
        CareerFlowManager.Instance.StartNewCareer();
    }

    public void ContinueCareer()
    {
        CareerFlowManager.Instance.ContinueCareer();
    }

    public void Quit()
    {
#if !UNITY_WEBGL
        // Browsers can't quit; consoles generally shouldn't offer it either (hide the button there).
        Application.Quit();
#endif
    }
}