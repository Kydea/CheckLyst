//
//  IstanceBindResponseJSON.swift
//  CheckListManager
//
//  Created by Simone Testa on 28/12/2017.
//  Copyright © 2017 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct IstanceBindResponseJSON: Decodable
{
    
    public let fields : Fields
    
    public let valid : Bool
    
    public let validationMessages : [String?]
    
}

public struct Fields : Decodable
{
    
    public let department : Field?
    
    public let description : Field?
    
    public let tagValue : Field?
    
    public let state : Field?
    
    public let prototypeType : Field?
    
    public let prototype : Field?
    
    public let version : Field?
    
    public let tagCode : Field?
    
}

public struct Field : Decodable
{
    
    public let value : String?
    
    public let editable : Bool
    
    public let visible : Bool
    
    public let options : [Option]
    
    public let validationMessages : [String?]
    
}

public struct Option : Decodable, Encodable
{
    
    public var text : String
    
    public var value : String
    
}
