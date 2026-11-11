using System.Security.Cryptography;
using Isopoh.Cryptography.Argon2;
using System.Text;

public static class EncodingSide {    
    public static void Main() {
        
    }
    public static void HashingExec() {
        Console.Write("Hello, Enter needed password for doing hashing: ");

        string password = Console.ReadLine() ?? "";
        string result = ArgonHashing(password); //Argon Is Recommended Algorithm for doing hashing
        Console.WriteLine("\nHashed Password: " + result);

        Console.Write("\nEnter your password for login: ");
        string enteredPassword = Console.ReadLine()??"";
        string verificationResult = VerifyArgonHashing(enteredPassword, result);
        Console.WriteLine(verificationResult);
    }
    public static string ArgonHashing(string password){
        try {
            byte[] salt = new byte[16]; // 16 * 8 = 128 bit
            RandomNumberGenerator.Fill(salt);
            string hashedPassword = DoArgonHashing(password, salt);
            return hashedPassword;
        }catch(Exception ex){
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    public static string DoArgonHashing(string password, byte[] salt){
        try {
            var config = new Argon2Config {
                //HybridAddressing: is the best and most common way for better memory consuming
                Type = Argon2Type.HybridAddressing,

                //[ Nineteen ] stable argon algorithm version
                Version = Argon2Version.Nineteen,

                //iteration time of mixing password with exist salt to generate strong hashed result and difficulty suggest it 
                TimeCost = 4,

                //[ Strong Section ] memory size consuming to create hashed password, where here it will take 128 megabytes
                MemoryCost = 128 * 1024,

                //Parallelism Part both : [ Lanes and Threads ]
                //Note: Must have same value 2 - 2 or 1 - 1 [ Recommended way ] for both Lanes and Threads and not difference!
                //Lanes: as size or lines within the RAM
                //Threads [ Executer ]: as number of threads that used by using CPU
                Lanes = 1,
                Threads = 1,
                
                // here main password as bytes and main salt
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt,

                HashLength = 32 // 256-bit hash output
            };
            string HashedPassword = Argon2.Hash(config);
            return HashedPassword;
        }catch (Exception ex){
            Console.WriteLine(ex.Message);
            throw;
        }
    }        
    public static string VerifyArgonHashing(string enteredPassword,string hashedPassword) {
        try {
            bool isValid = Argon2.Verify(hashedPassword, enteredPassword);
            return isValid ? "\nYour login is corrected" : "\nOpps, something went wrong!";
        }
        catch (Exception ex){
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    public static string ASHAHashing(string data){
        //the result 256 [ Fixed Sized String as a Signature ] bit even size of data is largest of 256
        try{
            using (SHA256 doHashing = SHA256.Create()){
                byte[] result = doHashing.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(result).Replace("-","").ToLower();
            }
        }catch(Exception ex){
            Console.WriteLine(ex.Message);
            throw;
        }
    }
    public static void RSAExec(){
        try {
            //The RSA Process: 
            //[ 1 ] create public and private key for encryption and decryption
            //[ 2 ] client side that used [ public key ] for doing encryption to the data 
            // Then we have now: Encrypted Message
            //[ 3 ] server side it will using [ private key ] for doing decryption 

            //RSACryptoServiceProvider: Class that response for implementing RSA Encryption algorithm
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider()){

                //Firstly, why we using XML?
                //to enable key to be as string that can be store it and move it over the network.                

                //The possibility of displaying the encrypted word in XML format to facilitate its transfer over the network.When we say that it is false, this is an indication that this part is a public key and true is a private key, right?

                string publicKey  = RSA.ToXmlString(false);
                string privateKey = RSA.ToXmlString(true);                

                Console.Write("\nEnter message that you want to encrypted it: ");
                string message = Console.ReadLine() ?? "";

                string encryptionResult = RSAEncryption(message,publicKey);
                Console.WriteLine("\nEncryption Result: " + encryptionResult);

                string decryptedResult = RSADecryption(encryptionResult, privateKey);
                Console.WriteLine("\nDecrypted Result: " + decryptedResult);
            }
        }
        catch (CryptographicException ex){
            Console.WriteLine("Cryptographic Exception: " + ex.Message);
        }catch (Exception ex) {
            Console.WriteLine("Exception: " + ex.Message);
        }
    }
    public static string RSAEncryption(string planText,string publicKey){
        try {
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider()){

                RSA.FromXmlString(publicKey);//it will download incoming public key for doing encryption correctly

                //Encoding.UTF8.GetBytes(planText): it will convert data into bytes, because algorithm that needed to work with bytes data not string

                
                //Padding is the process of adding specific, secure, random data to your original text before sending it to the encryption machine.

                //The idea is to [ add padding ] around the text to fill in any gaps and make the envelope look secure, preventing anyone from guessing its true contents or tampering with it.
                
                //false that mean: PKCS#1 old version of padding
                //true that mean :OAEP Optimal Asymmetric Encryption Padding [ Recommended Way ]

                byte[] encryptedData = RSA.Encrypt(Encoding.UTF8.GetBytes(planText), true);

                return Convert.ToBase64String(encryptedData);
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("Cryptographic Exception: " + ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            throw;

        }
    }
    public static string RSADecryption(string cipherText , string privateKey){
        try {

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider()){
                RSA.FromXmlString(privateKey);

                byte[] encryptedData = Convert.FromBase64String(cipherText);
                byte[] decryptedData = RSA.Decrypt(encryptedData, true);
                return Encoding.UTF8.GetString(decryptedData);
                //return Convert.ToBase64String(decryptedData);
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("Cryptographic Exception: " + ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex.Message);
            throw;
        }
    }
    public static void AESExec(){
        Console.WriteLine("Please enter a username:");
        string username = Console.ReadLine() ?? "";

        Console.WriteLine("Please enter a password:");
        string password = Console.ReadLine() ?? "";

        //the system it will use key + IV to build strong random and difficult predictable both characters and numbers for encryption 
        byte[] key = new byte[16];
        byte[] iv = new byte[16]; //witch numbers be as valid size rather than 16 when i want to hold largest or long result ?

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(key);//store random and difference values into key and iv
            rng.GetBytes(iv);
        }

        //Encrypt the password
        byte[] encryptedPassword = AESEncryption(password, key, iv);
        string encryptedPasswordString = Convert.ToBase64String(encryptedPassword);        

        Console.WriteLine("Encrypted password: " + encryptedPasswordString);

        //Decrypt the password
        string decryptedPassword = AESDecryption(encryptedPassword, key, iv);
        Console.WriteLine("Decrypted password: " + decryptedPassword);
    }
    public static byte[] AESEncryption(string simpleText, byte[] key, byte[] iv) {
        byte[] cipheredText;
        try {
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                // ICryptoTransform: interface tool it [ will doing data encryption ] by using AES Algorithm
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    //[ 1 ] StreamWriter: received data then convert it into bytes
                    //[ 2 ] CryptoStream: move data into encryptor then send result into memory
                    //[ 3 ] memoryStream: store encryption result into RAM here rather than file or another way
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(simpleText);
                        }
                        cipheredText = memoryStream.ToArray();//[ 4 ] Finally moved encrypted result into cipheredText
                    }
                }
            }
        }
        catch (CryptographicException ex){
            Console.WriteLine("Cryptographic Exception: " + ex.Message);throw;
        }
        catch (Exception ex){
            Console.WriteLine("Exception: " + ex.Message);throw;
        }
        return cipheredText;
    }
    public static string AESDecryption(byte[] cipheredText, byte[] key, byte[] iv){
        string simpleText = string.Empty;
        try
        {
            using (Aes aes = Aes.Create())
            {
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                using (MemoryStream memoryStream = new MemoryStream(cipheredText))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            simpleText = streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (CryptographicException ex){
            Console.WriteLine("Cryptographic Exception: " + ex.Message);
        }
        catch (Exception ex){
            Console.WriteLine("Exception: " + ex.Message);
        }
        return simpleText;
    }
    public static void UTFEncoding(){
        //this process it will handled by default by serialization and https when we want to data transfer in api
        //UTF: convert text into bytes
        //it can represent al most characters
        string text = "Hello, احمد!";
        byte[] encoding = Encoding.UTF8.GetBytes(text);
        // also the computer that worked with data as bytes to be understandable for it, so we want to convert data into bytes not remain it as string
        Console.WriteLine($"UTF-8 byte count: {encoding.Length}");

        //hex: readable format
        //X/x: show capital of small format to chars
        //2: show just two bit like A2 not A222
        Console.WriteLine("Bytes (hex) "+ string.Join(" ", encoding.Select(b => b.ToString("X2")))); //Is This code represent encoding?

        string decoding = Encoding.UTF8.GetString(encoding);
        Console.WriteLine("De-Coding: "+ decoding);
    }
    public static void Base64Handling(){
        //this section when we want to handle data as file / images / vides ... 
        string message = "welcome in 64 data safety provided";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        string encoding = Convert.ToBase64String(messageBytes);
        
        Console.WriteLine("Original Message: " + message);
        Console.WriteLine("Encoding: " + encoding);

        byte[] decoding = Convert.FromBase64String(encoding);
        string backToText = Encoding.UTF8.GetString(decoding);
        Console.WriteLine("De Coding: " + backToText);
    }
    public static void URLEncoding(){
        string url = "Ahmed & Admin";
        //That enable converting special character into safe representation
        string encoding = Uri.EscapeDataString(url);
        Console.WriteLine("Show encoding url: " + encoding);
        string decoding = Uri.UnescapeDataString(encoding);
        Console.WriteLine("Show decoding result: " + decoding);

    }
}