import { createContext, useContext, type ReactNode } from "react";
import type { CurrentUser } from "../types";

const CurrentUserContext = createContext<CurrentUser | null>(null);

export function CurrentUserProvider({currentUser, children} : { currentUser: CurrentUser; children: ReactNode; }) {
    return (
        <CurrentUserContext.Provider value={currentUser}>
            {children}
        </CurrentUserContext.Provider>
    );
}

export function useCurrentUser(): CurrentUser {
    const currentUser = useContext(CurrentUserContext);

    if (!currentUser) {
        throw new Error("useCurrentUser must be used inside CurrentUserProvider");
    }

    return currentUser;
}
