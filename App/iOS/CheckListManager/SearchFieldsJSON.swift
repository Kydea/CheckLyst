//
//  SearchJSON.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 01/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

public struct SearchFieldsJSON: Decodable

{
    
    public let ok: Bool
    
    public let companies: [String]?
    
    public let departments: [String]?
    
    public let prototypeTypes: [String]?
    
    public let prototype: [String]?
    
    public let tags: [String]?
    
    public let versions: [Version]?
    
}
