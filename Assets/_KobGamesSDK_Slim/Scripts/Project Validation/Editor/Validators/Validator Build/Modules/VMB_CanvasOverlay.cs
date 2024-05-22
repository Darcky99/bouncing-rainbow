using System.Linq;
// using KobGamesSDKSlim.Rendering;
using UnityEngine;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
    public class VMB_CanvasOverlay : ValidatorModuleBuild
    {
        private const string c_CanvasOverlayError = "Due to a Unity bug with URP we can't use Canvas Mode = Overlay. Use - Screen Space Canvas. " +
                                                    "If you can't assign the camera (ex: being a Prefab) add this script to the Canvas Object - ForceCanvasRenderCameraSpace";

        public override eBuildValidatorResult Validate()
        {
            //Skip if not using URP since URP is the problem
            // if (GameSettings.Instance.General.RenderingPipeline == eRenderingPipeline.BuiltIn)
            //     return eBuildValidatorResult.AllGood;
            
            //Get All Canvas with Render Mode - Overlay and that won't have ForceCanvasRenderCameraSpace script
            //Note - if we have hierarchy with multiple canvas, Only the root canvas should have the script
            
            var canvasList = ValidatorBuild.EvaluatedObjectsList
                                           .Select(objects => objects.GetComponent<Canvas>())
                                           .Where(canvas => canvas != null                                              
                                                         && canvas.renderMode                                   == RenderMode.ScreenSpaceOverlay
                                                         && canvas.GetComponent<ForceCanvasRenderCameraSpace>() == null
                                                         && !checkParentContainsOverlayCanvas(canvas))
                                           .ToList();
            
            //Show Errors
            canvasList.ForEach(x => showLogWithHierarchyPath(c_CanvasOverlayError, x.gameObject, true));
            
            if (canvasList.Count != 0)
                return eBuildValidatorResult.ErrorsNeedFixing;

            return eBuildValidatorResult.AllGood;
        }

        private bool checkParentContainsOverlayCanvas(Canvas i_Canvas) => i_Canvas.GetComponentsInParent<Canvas>().ToList()
                                                                                  .Where(parentCanvas => parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                                                                                  .Any(parent => parent                          != i_Canvas);
    }
}