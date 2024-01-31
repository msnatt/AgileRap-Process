namespace AgileRap_Process_Software_ModelV2
{
    public static class GlobalVariable
    {
        private static int UserID;
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
