#if ENABLE_URP

using KobGamesSDKSlim.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KobGamesSDKSlim.ProjectValidator.Modules.Build
{
	public class VMB_QualitySettings : ValidatorModuleBuild
	{
		public override eBuildValidatorResult Validate()
		{
			bool allGood          = true;
			bool containsWarnings = false;

			var renderPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;

			//TODO - we should analyze Quality Settings for Built-In pipeline
			if (renderPipeline == null || GameSettings.Instance.General.RenderingPipeline == eRenderingPipeline.BuiltIn)
			{
				Debug.LogError("Validator_QualitySettings skip. Using Built In Pipeline or QualitySettings.renderPipeline = null");
				return eBuildValidatorResult.AllGood;
			}

			//HQ Asset is used for HQ Screenshots so we shouldn't use it
			if (renderPipeline.name == "URP_Asset HQ")
			{
				Debug.LogError("Quality Settings is using the incorrect URP Asset. Make sure to use normal version and not the HQ");
				return eBuildValidatorResult.ErrorsNeedFixing;
			}

			if (renderPipeline.msaaSampleCount != (int)MsaaQuality.Disabled)
			{
				Debug.LogError("Quality Settings have issues. Make sure MSAA is set to Disabled.");
				allGood = false;
			}

			if (renderPipeline.renderScale > 1)
			{
				Debug.LogError("Quality Settings have issues. Make sure Render Scale is <= 1");
				allGood = false;
			}

			if (renderPipeline.mainLightShadowmapResolution == (int)UnityEngine.ShadowResolution.VeryHigh)
			{
				Debug.LogError("Quality Settings have issues. Make sure Shadow Res is <= 2048");
				allGood = false;
			}

			if (renderPipeline.additionalLightsRenderingMode == LightRenderingMode.PerPixel)
			{
				const string warning = "Quality Settings have a potential issue. Ideally Additional Light Rendering Mode shouldn't be Per Pixel";
				Debug.LogWarning(warning);
				ValidatorBuild.WarningsList.Add(warning);
				containsWarnings = true;
			}

			if (renderPipeline.shadowCascadeCount > 1)
			{
				const string warning = "Quality Settings have a potential issue. Ideally Shadow Cascade should be 1";
				Debug.LogWarning(warning);
				ValidatorBuild.WarningsList.Add(warning);
				containsWarnings = true;
			}

			if (!allGood)
				return eBuildValidatorResult.ErrorsNeedFixing;

			if (containsWarnings)
				return eBuildValidatorResult.WarningsOnly;

			return eBuildValidatorResult.AllGood;
		}
	}
}
#endif