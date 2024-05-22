using System;

namespace KobGamesSDKSlim.ProjectValidator
{
    public class ValidatorModuleOrderAttribute : Attribute
    {
        public readonly int Order;  
  
        public ValidatorModuleOrderAttribute(int i_Order)
        {
            Order = i_Order;
        }  
    }
}