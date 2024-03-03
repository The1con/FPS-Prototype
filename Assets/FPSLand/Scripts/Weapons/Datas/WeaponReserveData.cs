
namespace FirstGearGames.FPSLand.Weapons
{

    public struct WeaponReserveData
    {
        public WeaponNames WeaponName;
        public int Quantity;
        public bool AddToInventory;

        public WeaponReserveData(WeaponNames weaponName, int quantity, bool addToInventory)
        {
            WeaponName = weaponName;
            Quantity = quantity;
            AddToInventory = addToInventory;
        }
    }


}