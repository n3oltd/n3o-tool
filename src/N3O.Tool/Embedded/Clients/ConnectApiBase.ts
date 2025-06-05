export interface IApiHeaders {
    /**
     * Returns a valid bearer token for use in the Authorization header.
     * Used to dynamically inject the header.
     */
    getBearerToken?: () => string;
    
    /**
     * Returns a valid value for the N3O Subscription ID header.
     * Used to dynamically inject the header.
     */
    getSubscriptionId: () => string;
}

export class ConnectApiBase {
    private readonly apiHeaders: IApiHeaders;

    protected constructor(apiHeaders: IApiHeaders) {
        this.apiHeaders = apiHeaders;
    }

    protected transformOptions = (options: RequestInit): Promise<RequestInit> => {
        let headers: {[key: string]: string} = {
            ...options.headers as Record<string, string>
        }

        let subscriptionId = this.apiHeaders.getSubscriptionId();
        
        if (subscriptionId) {
            headers['N3O-Subscription-Id'] = subscriptionId;
        }

        let bearerToken = this.apiHeaders.getBearerToken?.();

        if (bearerToken) {
            headers['Authorization'] = bearerToken;
        }

        options.headers = headers;
        
        return Promise.resolve(options);
    };
}