//
//  Signer.swift
//  Checklyst
//
//  Created by Matematici Developer on 09/06/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct Signer: Codable {
    
    init(json:[String:Any]) {
        self.code = json["code"] as! String
        self.accessToken = json["access_token"] as! String
        self.refreshToken = json["refresh_token"] as! String
        self.roles = json["roles"] as! String
        self.name = json["name"] as! String
        self.accessTokenExpiration = (Date() + (json["expires_in"] as! TimeInterval)).timeIntervalSince1970.rounded()
    }
    
    init(usercode: String, accessToken: String, refreshToken: String, roles: String, name: String, accessTokenExpiration: TimeInterval) {
        self.code = usercode
        self.accessToken = accessToken
        self.refreshToken = refreshToken
        self.roles = roles
        self.name = name
        self.accessTokenExpiration = accessTokenExpiration
    }
    
    public var code: String
    public var name: String
    public var roles: String
    public var accessTokenExpiration: Double
    public var accessToken: String
    public var refreshToken: String
    
}
