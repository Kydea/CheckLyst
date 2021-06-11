//
//  URL+Utility.swift
//  Checklyst
//
//  Created by Simone Testa on 30/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension URL {
    
    enum ChecklystURLTypes : String {
        case Create, Detail, Setup, Filters, Undefined
    }
    
    var schemeHost: String? {
        guard let scheme = self.scheme, let host = self.host else {
            return nil
        }
        return scheme + "://" + host
    }
    
    var queryParameters: [String: String]? {
        guard
            let components = URLComponents(url: self, resolvingAgainstBaseURL: true),
            let queryItems = components.queryItems else { return nil }
        return queryItems.reduce(into: [String: String]()) { (result, item) in
            result[item.name] = item.value
        }
    }
    
    var getParametersForInstance: [String: String]? {
        var urlItems : Dictionary<String, String> = Dictionary()
        let values = self.path.split(separator: "/")
        let queryParametersString = self.absoluteString.split(separator: "?")
        let fieldValues = queryParametersString.count == 2 ? String(queryParametersString[1]) : nil
        if values.contains("Create") {
            if let index = values.index(of: "Create") {
                urlItems["prototype"] = values.count > 2 ? String(values[index + 1]) : ""
                urlItems["version"] = values.count > 3 ? String(values[index + 2]) : ""
                urlItems["fieldsValues"] = fieldValues ?? ""
            }
        } else if values.contains("Detail") {
            if let index = values.index(of: "Detail") {
                urlItems["code"] = values.count > 2 ? String(values[index + 1]) : ""
            }
        } else if values.contains("setup") {
            if let index = values.index(of: "setup") {
                urlItems["schema"] = values.count > 2 ? String(values[index + 1]) : "https"
            }
        }
        
        return urlItems
    }
    
    var getCallType : ChecklystURLTypes {
        switch(self.path.split(separator: "/")) {
            case let values where values.contains("Create"):
                return .Create
            case let values where values.contains("Detail"):
                return .Detail
            case let values where values.contains("setup"):
                return .Setup
            case let values where values.contains("List"):
                return .Filters
            default:
                return .Undefined
        }
        
    }
    
    var typeIdentifier: String? {
        return (try? resourceValues(forKeys: [.typeIdentifierKey]))?.typeIdentifier
    }
    var localizedName: String? {
        return (try? resourceValues(forKeys: [.localizedNameKey]))?.localizedName
    }
}
