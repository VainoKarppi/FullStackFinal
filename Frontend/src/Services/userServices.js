import axiosInstance from '../axiosInstance';


export const registerUser = async (formData) => {
    const response = await axiosInstance.post("/register", formData);

    if (!response.data.sessionToken) throw new Error("No session token received!");
    if (!response.data.tokenExpirationUTC) throw new Error("No session expiration received!");

    sessionStorage.setItem("sessionToken", response.data.sessionToken);
    sessionStorage.setItem("tokenExpirationUTC", response.data.tokenExpirationUTC);
};

export const loginUser = async (formData) => {
    try {
        const response = await axiosInstance.post("/login", formData);
        sessionStorage.setItem("sessionToken", response.data.sessionToken);
        sessionStorage.setItem("tokenExpirationUTC", response.data.tokenExpirationUTC);
        return true;
    } catch (error) {
        console.error('Error:', error);
        throw error;
    }
};


export const logoutUser = async () => {
    try {
        const token = sessionStorage.getItem("sessionToken");
        if (!token) return true;

        await axiosInstance.post("/logout", null, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        sessionStorage.clear("sessionToken");
        sessionStorage.clear("tokenExpirationUTC");
        return true;
    } catch (error) {
        console.error('Error:', error);
        return false;
    }
}

export const updateUser = async (updatedAccount) => {
    try {
        const token = sessionStorage.getItem("sessionToken");
        if (!token) throw new Error("Token not defined");

        await axiosInstance.patch("/user/update", JSON.stringify(updatedAccount), {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
    } catch (error) {
        console.error(error);
        throw error;
    }
}


export const userAuthorized = async () => {
    try {
        const token = sessionStorage.getItem("sessionToken");
        const sessionTokenExpiresUTC = new Date(sessionStorage.getItem("tokenExpirationUTC"));
        
        // Check if token and expirations are found
        if (!token || !sessionTokenExpiresUTC) return false;

        // Validate expiration time. Return true if success so no need to recheck from server
        if (sessionTokenExpiresUTC > new Date()) return true;

        // Login again if the expiration time of the token has passed -> Using token
        const response = await axiosInstance.post("/login", null, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        // Make sure response has the new token values
        if (!response.data.tokenExpirationUTC) return false;
        if (!response.data.sessionToken) return false;

        // Update token and expiration
        sessionStorage.setItem("sessionToken", response.data.sessionToken);
        sessionStorage.setItem("tokenExpirationUTC", response.data.tokenExpirationUTC);
        
        return true;
    } catch (error) {
        console.error("ASDASDASD:",error);
        if (error.request) {
            console.error("CLEARING SESSION");
            sessionStorage.clear("sessionToken");
            sessionStorage.clear("tokenExpirationUTC");
        }
        return false;
    }
}


