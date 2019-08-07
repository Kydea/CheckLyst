//
//  Company.swift
//  CheckListManager
//
//  Created by Simone Testa on 27/12/2017.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct Company: Decodable
{
    
    init(id : Int, name : String, inactivityTimeoutSeconds : Int) {
        self.id = id
        self.name = name
        self.inactivityTimeoutSeconds = inactivityTimeoutSeconds
    }
    
    public let id : Int
    
    public let name : String
    
    public let inactivityTimeoutSeconds : Int
    
}

