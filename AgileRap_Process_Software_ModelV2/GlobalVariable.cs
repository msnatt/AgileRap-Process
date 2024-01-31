namespace AgileRap_Process_Software_ModelV2
{
    public static class GlobalVariable
    {
        public static int UserID { get; set; }
        public static void SetUserLogin(int id)
        {
            UserID = id;
        }
        public static int GetUserLogin()
        {
            return UserID;
        }
    }
}
