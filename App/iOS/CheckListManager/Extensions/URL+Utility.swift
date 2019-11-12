//
//  URL+Utility.swift
//  Checklyst
//
//  Created by Simone Testa on 30/07/2019.
//  Copyright © 2019 Johnson&Johnson. All rights reserved.
//

import Foundation

extension URL {
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
        if values.contains("Create") {
            if let index = values.index(of: "Create") {
                urlItems["prototype"] = values.count > 2 ? String(values[index + 1]) : ""
                urlItems["version"] = values.count > 3 ? String(values[index + 2]) : ""
            }
        } else if values.contains("Detail") {
            if let index = values.index(of: "Detail") {
                urlItems["code"] = values.count > 2 ? String(values[index + 1]) : ""
            }
        }
        
        return urlItems
    }
    
    var typeIdentifier: String? {
        return (try? resourceValues(forKeys: [.typeIdentifierKey]))?.typeIdentifier
    }
    var localizedName: String? {
        return (try? resourceValues(forKeys: [.localizedNameKey]))?.localizedName
    }
}
