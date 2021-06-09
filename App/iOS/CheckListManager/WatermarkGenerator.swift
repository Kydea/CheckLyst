//
//  WatermarkGenerator.swift
//  Checklyst
//
//  Created by Matematici Developer on 31/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation
import UIKit

final class WatermarkGenerator {
    
    static func getWatermark(size:CGSize) -> UIImage? {
        let renderer = UIGraphicsImageRenderer(size: size)
        
        let image = renderer.image { ctx in
            let paragraphStyle = NSMutableParagraphStyle()
            paragraphStyle.alignment = .center
            
            let attrs: [NSAttributedString.Key: Any] = [
                .font: UIFont.systemFont(ofSize: 22),
                .paragraphStyle: paragraphStyle
            ]
            
            let attributedString = NSAttributedString(string: Formatter.instance.dateToDateTimeString(Date()), attributes: attrs)
            let stringSize = attributedString.size()
            
            attributedString.draw(with: CGRect(x: 0, y: size.height - stringSize.height, width: size.width, height: stringSize.height), options: .usesLineFragmentOrigin, context: nil)
            
            let logo = UIImage(named: "logo")
            logo?.draw(in: CGRect(x: 0, y: 0, width: size.width, height: size.height - stringSize.height))
        }
       
        return image
    }
}
